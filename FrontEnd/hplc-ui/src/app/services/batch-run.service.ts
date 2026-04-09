import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';
import { BatchRunInfo } from '../models/batch-run-info.model';
import { SampleExecutionInfo } from '../models/sample-execution.model';

@Injectable({ providedIn: 'root' })
export class BatchRunService {

  // ✅ BASE URL MATCHES BatchController ROUTES
  private api = 'https://localhost:7260/api/batch';

  private queueSubject = new BehaviorSubject<BatchRunInfo[]>([]);
  queue$ = this.queueSubject.asObservable();

  constructor(private http: HttpClient) {}

  // =========================
  // QUEUE
  // =========================

  loadQueue() {
    this.http.get<BatchRunInfo[]>(`${this.api}/queue`)
      .subscribe(q => this.queueSubject.next(q));
  }

  enqueue(batchName: string) {
    return this.http.post(`${this.api}/enqueue/${batchName}`, {})
      .pipe(tap(() => this.loadQueue()));
  }

  remove(batchName: string) {
    return this.http.delete(`${this.api}/${batchName}`)
      .pipe(tap(() => this.loadQueue()));
  }

  start(batchName: string) {
    return this.http.post(`${this.api}/start/${batchName}`, {})
      .pipe(tap(() => this.loadQueue()));
  }

  clear() {
    return this.http.post(`${this.api}/clear`, {})
      .pipe(tap(() => this.loadQueue()));
  }

  // =========================
  // AVAILABLE BATCHES
  // =========================

  // ✅ SEPARATE ENDPOINT FOR DEFINITIONS
  getAllBatches() {
    return this.http.get<string[]>(
      'https://localhost:7260/api/batch-definitions'
    );
  }

  // =========================
  // SAMPLE EXECUTION
  // =========================

  getExecution(batchName: string) {
    return this.http.get<SampleExecutionInfo[]>(
      `${this.api}/${batchName}/samples`
    );
  }
}