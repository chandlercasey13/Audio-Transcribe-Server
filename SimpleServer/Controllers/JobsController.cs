using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using SimpleServer.Data;
using SimpleServer.Models; 
using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace SimpleServer.Controllers
{
    [RoutePrefix("api/jobs")]
    public class JobsController : ApiController
    {
        private readonly IAmazonSQS _sqs;
        private readonly string _queueUrl;
        private readonly string _defaultGroup;

        public JobsController()
        {
            var region = RegionEndpoint.GetBySystemName(
                ConfigurationManager.AppSettings["AWSRegion"] ?? "us-east-2");

            // Uses EC2 instance role / env creds
            _sqs = new AmazonSQSClient(region);

            _queueUrl = ConfigurationManager.AppSettings["SqsQueueUrl"];
            _defaultGroup = ConfigurationManager.AppSettings["DefaultMessageGroupId"] ?? "default-group";
        }

        /// <summary>
        /// Enqueue a job into your SQS **FIFO** queue.
        /// POST /api/jobs/enqueue
        /// Body:
        /// {
        ///   "jobType": "AUDIO_TRANSCRIBE",
        ///   "s3Bucket": "my-bucket",
        ///   "s3Key": "uploads/clip.mp3",
        ///   "messageGroupId": "transcribe-group-1",     // optional
        ///   "deduplicationId": "unique-123"             // optional if content-based dedup is ON
        /// }
        /// </summary>
        [AllowAnonymous]
        [HttpPost, Route("enqueue")]
        public async Task<IHttpActionResult> Enqueue([FromBody] EnqueueJobRequest req)
        {
            if (string.IsNullOrWhiteSpace(_queueUrl))
                return Content(HttpStatusCode.InternalServerError, new { error = "SqsQueueUrl is missing in Web.config appSettings." });

            if (req == null || string.IsNullOrWhiteSpace(req.JobType))
                return Content(HttpStatusCode.BadRequest, new { error = "JobType required" });

            var jobId = Guid.NewGuid().ToString("N");
            var envelope = new
            {
                id = jobId,
                type = req.JobType.Trim(),
                s3Bucket = req.S3Bucket,
                s3Key = req.S3Key,
                requestedBy = User?.Identity?.Name ?? "anonymous",
                createdAt = DateTime.UtcNow
            };

            var body = JsonConvert.SerializeObject(envelope);

            var sendReq = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = body,

                // FIFO requirements:
                MessageGroupId = string.IsNullOrWhiteSpace(req.MessageGroupId)
                    ? _defaultGroup
                    : req.MessageGroupId.Trim(),

                // If your queue does NOT have content-based dedup enabled,
                // you must provide a DeduplicationId. Using jobId is fine.
                MessageDeduplicationId = string.IsNullOrWhiteSpace(req.DeduplicationId)
                    ? jobId
                    : req.DeduplicationId.Trim()
            };

            try
            {
                var resp = await _sqs.SendMessageAsync(sendReq);

                return Content(HttpStatusCode.Accepted, new
                {
                    enqueued = true,
                    jobId,
                    messageId = resp.MessageId
                });
            }
            catch (Exception ex)
            {
                var msg = (ex.InnerException ?? ex).Message;
                return Content(HttpStatusCode.InternalServerError, new { error = msg });
            }
        }
        [HttpPost]
        public async Task<IHttpActionResult> CreateJob([FromBody] CreateJobRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.S3Key))
                return BadRequest("S3Key required");

            var userId = 0; // TODO: replace with real user id (or make nullable)
            var jobId = Guid.NewGuid();

            try
            {
                using (var db = new AppDbContext())
                {
                    db.Jobs.Add(new Job
                    {
                        Id = jobId,                // omit if DB generates UUID
                        UserId = userId,           // ensure valid FK or make nullable
                        S3InputKey = req.S3Key,    // map to s3_input_key in entity
                        Status = "queued",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
                return Ok(new { jobId });
            }
            catch (Exception ex)
            {
                var root = ex;
                while (root.InnerException != null) root = root.InnerException;
                return Content(HttpStatusCode.InternalServerError, new { error = root.Message });
            }
        }

        public class CreateJobRequest
        {
            public string S3Key { get; set; }
        }
    }
}
