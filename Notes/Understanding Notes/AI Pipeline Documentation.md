# Smart AI Dashboard - AI Pipeline Documentation

---

# GenerateTextCommandHandler

```csharp
/*
GenerateTextCommandHandler Working:
1. Receive GenerateTextCommand from MediatR.
2. Create a new AIJob with JobType=GenerateText and Status=Pending.
3. Save the AIJob to the database through IAIJobStore.
4. Enqueue the AIJob into IAIQueue for background processing by AIWorker.
5. Return AIOperationResponse containing JobId and current Status.
*/
```

**Responsibility**

- Starts an asynchronous AI generation job.
- Creates the AIJob but **does not call Gemini directly**.
- Hands the job to the background worker.

---

# SummarizeCommandHandler

```csharp
/*
SummarizeCommandHandler Working:
1. Receive SummarizeCommand from MediatR.
2. Create an AIRequest with JobType=Summarize.
3. Call IAIService.ProcessAsync() to process the request.
4. Convert AIProviderResponse into AIOperationResponse.
5. Return AIOperationResponse to the controller.
*/
```

**Responsibility**

- Executes summarization immediately.
- Uses the common AI pipeline.
- No background worker is involved.

---

# AIRequest

```csharp
/*
AIRequest Purpose:
- Internal request model used by AIService.
- Represents any AI operation together with its input.
- Every AI operation (GenerateText, Summarize, etc.) is converted into an AIRequest.
*/
```

**Responsibility**

Acts as the common request model used internally by AIService regardless of which AI operation is being performed.

---

# AiService

```csharp
/*
AiService Working:
1. Receive AIRequest from IAIService.
2. Generate cache key using JobType and Input.
3. Try Redis cache (Fault-tolerant).
4. If cache misses, build provider prompt based on JobType.
5. Call IAIProvider.GenerateAsync() to get AI response.
6. Store successful response in Redis (Fault-tolerant).
7. Return AIProviderResponse containing Output and IsFallback.
*/
```

**Responsibility**

Acts as the orchestrator of the AI pipeline.

It coordinates:

- Cache
- Prompt building
- AI Provider
- Response mapping

without knowing which provider (Gemini, OpenAI, Claude, etc.) is actually being used.

---

# GeminiProvider

```csharp
/*
GeminiProvider Working:
1. Receive prompt from AiService through IAIProvider.
2. Read Gemini API key from configuration.
3. Create AIClient using IHttpClientFactory.
4. Build GeminiRequest with the prompt.
5. Send HTTP request to Gemini API.
6. Log response status and response body.
7. Extract generated text from GeminiResponse.
8. Return generated text.
9. If any exception occurs, log the error and return an error message.
*/
```

**Responsibility**

Only communicates with the Gemini API.

It contains **no business logic**, cache logic, or controller logic.

---

# AIProviderResponse

```csharp
/*
AIProviderResponse Purpose:
- Internal response model returned by AIService.
- Contains provider-specific information.
- Not exposed directly to the frontend.
*/
```

**Responsibility**

Represents the response produced by the AI provider.

Currently contains:

- Output
- IsFallback

---

# AIOperationResponse

```csharp
/*
AIOperationResponse Purpose:
- Standard response returned to the frontend.
- Shared response model for all AI operations.
- Can represent both asynchronous and synchronous AI operations.
*/
```

**Responsibility**

This is the DTO returned by every AI endpoint.

Examples:

- Generate
- Summarize
- Future Chat
- Future Translate
- Future RAG

---

# AIQueue

```csharp
/*
AIQueue Purpose:
- Temporary in-memory queue for AI jobs.
- GenerateTextCommandHandler enqueues jobs.
- AIWorker dequeues and processes jobs in the background.
*/
```

**Responsibility**

Temporarily stores AI jobs until the background worker processes them.

---

# AIWorker

```csharp
/*
AIWorker Working:
1. Continuously monitor AIQueue for new jobs.
2. Dequeue the next AIJob.
3. Update AIJob status to Processing and save to database.
4. Create an AIRequest from the AIJob.
5. Call IAIService.ProcessAsync().
6. Store AI result, update status to Completed and save to database.
7. If processing fails:
   - Increase RetryCount.
   - Log the failure.
   - If retries remain:
       • Update status to Retrying.
       • Save to database.
       • Enqueue the job again.
   - Otherwise:
       • Update status to Failed.
       • Save error message and CompletedAt.
       • Save to database.
       • Log the final failure.
*/
```

**Responsibility**

Processes asynchronous AI jobs independently of HTTP requests.

---

# AIJobStore

```csharp
/*
AIJobStore Purpose:
- Responsible only for AIJob database operations.
- Provides CRUD operations for AIJob entities.
- Contains no AI processing or business logic.
*/
```

**Responsibility**

Acts as the persistence layer for AI jobs.

Only reads and writes AIJob entities to the database.

---

# AIController

```csharp
/*
AIController Purpose:
- Entry point for AI HTTP endpoints.
- Receives client requests.
- Sends Commands/Queries through MediatR.
- Returns standardized API responses.
- Contains no business logic.
*/
```

**Responsibility**

The controller only accepts HTTP requests and forwards them to the Application layer using MediatR.

It never talks directly to:

- AIService
- GeminiProvider
- Database
- Redis

---

# Overall AI Architecture

```
Client
    │
    ▼
AIController
    │
    ▼
MediatR
    │
    ▼
Command / Query Handler
    │
    ├────────────── GenerateText
    │                   │
    │                   ▼
    │              Create AIJob
    │                   │
    │              Save to DB
    │                   │
    │              Enqueue Job
    │                   │
    │              AIWorker
    │                   │
    │                   ▼
    └────────────── Summarize
                        │
                        ▼
                  Create AIRequest
                        │
                        ▼
                 IAIService.ProcessAsync()
                        │
                        ▼
                     AiService
                        │
             ┌──────────┴──────────┐
             │                     │
             ▼                     ▼
         Redis Cache          Build Prompt
                                   │
                                   ▼
                          IAIProvider
                                   │
                                   ▼
                           GeminiProvider
                                   │
                                   ▼
                        AIProviderResponse
                                   │
                                   ▼
                        AIOperationResponse
                                   │
                                   ▼
                              AIController
                                   │
                                   ▼
                                Client
```

---

# Layer Responsibilities

| Layer | Responsibility |
|--------|----------------|
| API | Receives HTTP requests and returns responses |
| MediatR | Routes Commands and Queries to the correct Handler |
| Command Handler | Executes application business logic |
| AIRequest | Internal request model shared by all AI operations |
| AiService | Orchestrates cache, prompt building, and AI provider |
| IAIProvider | Abstract AI provider interface |
| GeminiProvider | Communicates with Gemini API |
| AIProviderResponse | Internal provider response |
| AIOperationResponse | Response returned to the frontend |
| AIQueue | Holds background AI jobs |
| AIWorker | Processes queued jobs asynchronously |
| AIJobStore | Handles database operations for AIJob |
| Database | Persists AI jobs and results |