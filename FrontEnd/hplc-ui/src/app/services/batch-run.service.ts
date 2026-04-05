import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BatchRunInfo } from './batch'; // <-- adjust path if needed

@Injectable({ providedIn: 'root' })
export class BatchRunService {

  // Keep this CONSISTENT with BatchService
  private baseUrl = 'https://localhost:7260/api/batch/run';

  constructor(private http: HttpClient) {}

  /**
   * GET /api/batch/run/queue
   */
  getQueue(): Observable<BatchRunInfo[]> {
    return this.http.get<BatchRunInfo[]>(
      `${this.baseUrl}/queue`
    );
  }

  /**
   * POST /api/batch/run/queue/{batchName}
   */
  addToQueue(batchName: string): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/queue/${batchName}`,
      {}
    );
  }
}