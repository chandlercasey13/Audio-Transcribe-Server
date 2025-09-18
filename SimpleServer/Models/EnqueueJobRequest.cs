namespace SimpleServer.Models
{
    public class EnqueueJobRequest
    {
        public string JobType { get; set; }          // e.g., "AUDIO_TRANSCRIBE"
        public string S3Bucket { get; set; }         // optional payload
        public string S3Key { get; set; }            // optional payload
        public string MessageGroupId { get; set; }   // FIFO requirement (can default in controller)
        public string DeduplicationId { get; set; }  // optional, unless Content-based dedup = off
    }
}
