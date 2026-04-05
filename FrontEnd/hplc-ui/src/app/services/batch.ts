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

export interface BatchRunInfo {
  batchName: string;
  status: string;
  queuePosition: number;
  batchStarted?: string;
  batchCompleted?: string;
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

  getAllBatches(): Observable<string[]> {
    return this.http.get<string[]>(`${this.api}/batch`);
  }

  getBatch(name: string): Observable<Batch> {
    return this.http.get<Batch>(`${this.api}/batch/${name}`);
  }

  saveBatch(payload: Batch): Observable<void> {
    return this.http.post<void>(`${this.api}/batch`, payload);
  }

  getMethods(): Observable<any[]> {
    return this.http.get<any[]>(`${this.api}/methods`);
  }

  getBatchRunQueue(): Observable<BatchRunInfo[]> {
    return this.http.get<BatchRunInfo[]>(
      `${this.api}/batch/run/queue`
    );
  }

  addBatchToRunQueue(name: string): Observable<void> {
    return this.http.post<void>(
      `${this.api}/batch/run/queue/${name}`, {}
    );
  }
}
