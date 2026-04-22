import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';

/* =========================
   Models
   ========================= */

export interface Sample {
  serialNo: number;
  sampleName: string;
  injectionLocation: string;
  injectionVolume: number;
  methodId: string;
}

export interface Batch {
  batchName: string;
  samples: Sample[];
}

export interface MsStatus {
  isAcquiring: boolean;
  activeLcId?: string;
  batchName?: string;
}

export interface BatchRunSummary {
  startTime: string;
  endTime: string;
  multiplexedDuration: string;
  sequentialDuration: string;
  timeSaved: string;
  percentImprovement: number;
}

export type BatchRunStatus =
  | 'Queued'
  | 'Running'
  | 'Completed';

export interface SampleExecutionInfo {
  sampleName: string;
  methodId: string;
  assignedLC: number;
  state:
    | 'Queued'
    | 'Preparing'
    | 'WaitingForMS'
    | 'Injecting'
    | 'Acquiring'
    | 'Completed';
}

export interface BatchRunInfo {
  batchName: string;
  status: BatchRunStatus;
  queuePosition: number;
  samples: SampleExecutionInfo[];
}

/* =========================
   Chromatograph models
   ========================= */

export interface ChromStatus {
  state: 'Idle' | 'Running';
  batch?: string;
  sample?: string;
  startTime?: string;
}

export interface ChromPoint {
  time: number;
  intensity: number;
}

/* =========================
   Service
   ========================= */

@Injectable({
  providedIn: 'root'
})
export class BatchService {

  private readonly api = 'https://localhost:7260/api';

  constructor(private http: HttpClient) {}

  /* =========================
     Batch definitions
     ========================= */

  /** GET /api/batch */
  getAllBatches(): Observable<string[]> {
    return this.http.get<string[]>(
      `${this.api}/batch`
    );
  }

  /** GET /api/batch/{batchName} */
  getBatch(batchName: string): Observable<Batch> {
    return this.http.get<Batch>(
      `${this.api}/batch/${batchName}`
    );
  }

  /** POST /api/batch/save */
  saveBatch(batch: Batch): Observable<void> {
    return this.http.post<void>(
      `${this.api}/batch/save`,
      batch
    );
  }

  /** GET /api/methods */
  getMethods(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.api}/methods`
    );
  }

  /* =========================
     Run Queue
     ========================= */

  /** GET /api/batch/queue */
  getBatchRunQueue(): Observable<BatchRunInfo[]> {
    return this.http.get<BatchRunInfo[]>(
      `${this.api}/batch/queue`
    );
  }

  /**
   * ✅ CORRECT enqueue call (Swagger-backed)
   * POST /api/batch/enqueue/{batchName}
   */
  enqueueBatchByName(batchName: string): Observable<void> {
    return this.http.post<void>(
      `${this.api}/batch/enqueue/${batchName}`,
      {}
    );
  }

  /** POST /api/batch/start/{batchName} */
  startBatch(batchName: string): Observable<void> {
    return this.http.post<void>(
      `${this.api}/batch/start/${batchName}`,
      {}
    );
  }

  /** DELETE /api/batch/queue */
  clearRunQueue(): Observable<void> {
    return this.http.delete<void>(
      `${this.api}/batch/queue`
    );
  }

  /* =========================
     MS Status
     ========================= */

  /** GET /api/status/ms */
  getMsStatus(): Observable<MsStatus> {
    return this.http.get<MsStatus>(
      `${this.api}/status/ms`
    );
  }

  /* =========================
     Live Chromatograph
     ========================= */

  /** GET /api/status/chromatogram-meta */
  getChromatographStatus(): Observable<ChromStatus> {
    return this.http.get<ChromStatus>(
      `${this.api}/status/chromatogram-meta`
    );
  }

  /** GET /api/status/chromatogram/{batch}/{sample} */
  getChromData(
    batch: string,
    sample: string
  ): Observable<ChromPoint[]> {
    return this.http.get<ChromPoint[]>(
      `${this.api}/status/chromatogram/${batch}/${sample}`
    );
  }

  /* =========================
     Optional / Legacy
     ========================= */

 
getBatchRunSummary() {
  return this.http.get<BatchRunSummary>(
    `${this.api}/batch/summary`
  );
}

  
}