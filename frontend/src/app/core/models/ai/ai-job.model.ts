export interface AIJob {
  id: string;
  jobType: string;
  status: string;
  prompt: string;
  createdAt: string;
  completedAt: string | null;
}