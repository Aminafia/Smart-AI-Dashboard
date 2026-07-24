import { AIJobStatus } from "./ai-job-status.model";

export interface JobStatusResponse {
  id: string;
  jobType: string;
  prompt: string;
  status: AIJobStatus;
  result: string | null;
  error: string | null;
  createdAt: string;
  completedAt: string | null;
}