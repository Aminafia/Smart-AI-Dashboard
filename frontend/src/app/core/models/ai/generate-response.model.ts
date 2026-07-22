import { AIJobStatus } from "./ai-job-status.model";

export interface GenerateResponse {
  jobId: string;
  status: AIJobStatus;
}