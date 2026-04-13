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
  assignedLC: number;
}

export interface Batch {
  batchName: string;
  samples: Sample[];
}

export type BatchRunStatus =
  | 'Queued'
  | 'Running'
  | 'Completed';

export interface SampleExecutionInfo {
  sampleName: string;
  methodId: string;
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

  getAllBatches(): Observable<string[]> {
    return this.http.get<string[]>(
      `${this.api}/batch`
    );
  }

  getBatch(batchName: string): Observable<Batch> {
    return this.http.get<Batch>(
      `${this.api}/batch/definition/${batchName}`
    );
  }

  saveBatch(batch: Batch): Observable<void> {
    return this.http.post<void>(
      `${this.api}/batch/save`,
      batch
    );
  }

  getMethods(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.api}/methods`
    );
  }

  /* =========================
     Run Queue
     ========================= */

  getBatchRunQueue(): Observable<BatchRunInfo[]> {
    return this.http.get<BatchRunInfo[]>(
      `${this.api}/batch/queue`
    );
  }

  enqueueBatch(batch: Batch): Observable<void> {
    return this.http.post<void>(
      `${this.api}/batch/enqueue`,
      batch
    );
  }

  startBatch(batchName: string): Observable<void> {
    return this.http.post<void>(
      `${this.api}/batch/start/${batchName}`,
      {}
    );
  }

  clearRunQueue(): Observable<void> {
    return this.http.delete<void>(
      `${this.api}/batch/queue`
    );
  }

  getBatchRunSummary(): Observable<any | null> {
    return of(null);
  }
}
