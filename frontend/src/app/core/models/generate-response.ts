import { AIJobStatus } from "./ai-job-status"; 

export interface GenerateResponse {
  jobId: string;
  status: AIJobStatus;
}