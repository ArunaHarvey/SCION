import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/* =========================
   Models
   ========================= */

export interface Sample {
  serialNo: number;
  sampleName: string;
  injectionLocation: string;
  injectionVolume: number;
  methodId: string;
  assignedLC: number;
}

export interface Batch {
  batchName: string;
  samples: Sample[];
}

/**
 * Backend-aligned batch run status
 */
export type BatchRunStatus =
  | 'Queued'
  | 'Injecting'
  | 'Running'
  | 'WaitingForMS'
  | 'Acquiring'
  | 'Completed';

/**
 * Scheduler truth object
 */
export interface BatchRunInfo {
  batchName: string;
  lcId: string;

  status: BatchRunStatus;

  ownsMS: boolean;
  queuePosition: number | null;

  // Timing (set by backend)
  injectionStartTime?: string;
  completionTime?: string;
  actualDuration?: string;
}

/**
 * Aggregate run comparison (multiplex vs sequential)
 */
export interface BatchRunSummary {
  startTime: string;
  endTime: string;

  multiplexedDuration: string;
  sequentialDuration: string;

  timeSaved: string;
  percentImprovement: number;
}

/* =========================
   Service
   ========================= */

@Injectable({
  providedIn: 'root'    // ✅ SINGLE SOURCE OF TRUTH
})
export class BatchService {

  private readonly api = 'https://localhost:7260/api';

  constructor(private http: HttpClient) {}

  /* =========================
     Batch definition APIs
     ========================= */

  getAllBatches(): Observable<string[]> {
    return this.http.get<string[]>(
      `${this.api}/batch`
    );
  }

  getBatch(name: string): Observable<Batch> {
    return this.http.get<Batch>(
      `${this.api}/batch/${name}`
    );
  }

  saveBatch(payload: Batch): Observable<void> {
    return this.http.post<void>(
      `${this.api}/batch`, payload
    );
  }

  getMethods(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.api}/methods`
    );
  }

  /* =========================
     Batch run / scheduler APIs
     ========================= */

  /**
   * Returns LIVE scheduler truth:
   * Queued / Injecting / Running / MS owning / Completed
   */
  getBatchRunQueue(): Observable<BatchRunInfo[]> {
    return this.http.get<BatchRunInfo[]>(
      `${this.api}/batch/run/queue`
    );
  }

  /**
   * Queue batch for execution.
   * NOTE: This does NOT directly start the run.
   * Scheduler decides when injection starts.
   */
  addBatchToRunQueue(batchName: string): Observable<void> {
    return this.http.post<void>(
      `${this.api}/batch/run/queue/${batchName}`, {}
    );
  }

  /**
   * Multiplex vs Sequential run summary
   */
  getBatchRunSummary(): Observable<BatchRunSummary | null> {
    return this.http.get<BatchRunSummary | null>(
      `${this.api}/batch/run/summary`
    );
  }
}
