export interface JobStatusResponse {
  id: string;
  status: string;
  result: string | null;
  error: string | null;
}