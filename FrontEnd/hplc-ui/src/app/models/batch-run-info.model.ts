export type BatchRunStatus =
  | 'Queued'
  | 'Injecting'
  | 'Running'
  | 'WaitingForMS'
  | 'Acquiring'
  | 'Completed';

export interface BatchRunInfo {
  batchName: string;
  lcId: string;

  status: BatchRunStatus;

  ownsMS: boolean;
  queuePosition: number | null;

  injectionStartTime?: string;
  completionTime?: string;
  actualDuration?: string;
}