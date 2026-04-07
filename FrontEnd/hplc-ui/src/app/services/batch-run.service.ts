import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';
import { BatchRunInfo } from '../models/batch-run-info.model';
import { SampleExecutionInfo } from '../models/sample-execution.model';

@Injectable({ providedIn: 'root' })
export class BatchRunService {

  private api = 'https://localhost:7260/api/batch/run';

  private queueSubject = new BehaviorSubject<BatchRunInfo[]>([]);
  queue$ = this.queueSubject.asObservable();

  constructor(private http: HttpClient) {}

  loadQueue() {
    this.http.get<BatchRunInfo[]>(`${this.api}/queue`)
      .subscribe(q => this.queueSubject.next(q));
  }

  getAllBatches() {
    return this.http.get<string[]>('https://localhost:7260/api/batch');
  }

  enqueue(batchName: string) {
    return this.http.post(`${this.api}/queue/${batchName}`, {})
      .pipe(tap(() => this.loadQueue()));
  }

  remove(batchName: string) {
    return this.http.delete(`${this.api}/queue/${batchName}`)
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

  getExecution() {
    return this.http.get<SampleExecutionInfo[]>(
      `${this.api}/execution`
    );
  }
}