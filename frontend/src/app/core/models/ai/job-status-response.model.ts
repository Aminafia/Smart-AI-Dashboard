import { AIJobStatus } from "./ai-job-status.model";

export interface JobStatusResponse {
  id: string;
  status: AIJobStatus;
  result: string | null;
  error: string | null;
}