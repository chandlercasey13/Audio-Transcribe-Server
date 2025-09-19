# Audio Transcriber App

Full-stack project that takes in audio files, transcribes them to text, and returns results back to the client.  
It’s built around AWS services, a .NET Web API 2 backend, and a Python/Whisper worker.

---

## 🚀 Tech Stack

- **Frontend:** React (static site)
- **Backend API:** ASP.NET Web API 2.0, hosted on Windows Server 2019 EC2
- **Storage:** Amazon S3 (file uploads via pre-signed URLs)
- **Queue:** Amazon SQS (FIFO)
- **Database:** Amazon RDS (Postgres)
- **Worker:** Python + [OpenAI Whisper] model, packaged in Docker, running on ECS Fargate

---

## 📐 Architecture Overview


## 🛠 How It Works (System Flow)

1. **Upload**  
   Client uploads an audio file directly to S3 using a pre-signed URL from the API.

2. **Queue Message**  
   Once uploaded, the API drops a message into the SQS FIFO queue with file metadata.

3. **Worker**  
   - A Python worker (Docker container on ECS Fargate) long-polls the queue.  
   - It fetches the file from S3, runs Whisper locally to transcribe it.  
   - Writes the text result back (e.g. DB or response payload depending on request).

4. **Result**  
   API retrieves the processed text from Postgres and returns it to the client.

---



## ⚙️ Components

- **React Frontend**: Handles user upload & displays transcription.
- **Web API 2.0**: Issues pre-signed URLs, enqueues jobs, exposes results.
- **S3**: Temporary audio storage.
- **SQS FIFO**: Ensures audio jobs are handled in order, no duplicates.
- **Worker**: Python service with Whisper model baked into the container.
- **RDS (Postgres)**: Stores job metadata + transcription results.

---





## 📄 License

MIT (or whatever you choose)
