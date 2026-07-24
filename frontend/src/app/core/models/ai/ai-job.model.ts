import { AIJobStatus } from "./ai-job-status.model";

export interface AIJob {
  id: string;
  jobType: string;
  status: AIJobStatus;
  prompt: string;
  createdAt: string;
  completedAt: string | null;
}