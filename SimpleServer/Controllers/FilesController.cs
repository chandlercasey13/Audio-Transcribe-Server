using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using System;
using System.Configuration;
using System.Web.Http;

[RoutePrefix("api/files")]
public class FilesController : ApiController
{
    private readonly string _bucket;
    private readonly IAmazonS3 _s3;

    public FilesController()
    {
        _bucket = ConfigurationManager.AppSettings["S3Bucket"];
        var region = RegionEndpoint.GetBySystemName(ConfigurationManager.AppSettings["AWSRegion"]);
        _s3 = new AmazonS3Client(region);
    }

    // GET /api/files/presign?key=uploads/clip.mp3
    [HttpGet, Route("presign")]
    public IHttpActionResult GetPresignedUrl(string key)
    {
        var req = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Verb = HttpVerb.PUT,                       // allows client upload
            Expires = DateTime.UtcNow.AddMinutes(15),  // 15 min expiration
            ContentType = "audio/mpeg"

        };

        var url = _s3.GetPreSignedURL(req);

        return Ok(new { uploadUrl = url, key });
    }

    // GET /api/files/presign-download?key=transcripts/clip-001.txt
    [HttpGet, Route("presign-download")]
    public IHttpActionResult GetPresignedDownloadUrl(string key)
    {
        var req = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(30)
        };
        var url = _s3.GetPreSignedURL(req);
        return Ok(new { downloadUrl = url, key });
    }

}
