export interface BatchRunInfo {
  batchName: string;
  batchStarted?: string;
  batchCompleted?: string;
  status: string;
  queuePosition: number;
}