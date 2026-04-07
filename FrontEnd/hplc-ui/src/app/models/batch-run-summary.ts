
export interface BatchRunSummary {
  startTime: string;
  endTime: string;

  multiplexedDuration: string;
  sequentialDuration: string;

  timeSaved: string;
  percentImprovement: number;
}
