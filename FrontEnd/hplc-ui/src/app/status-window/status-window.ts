import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { BatchService, BatchRunInfo } from '../services/batch';

@Component({
  selector: 'app-status-window',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="panel">
      <h3>Batch Execution Status</h3>

      <table class="batch-table" *ngIf="runs.length > 0">
        <thead>
          <tr>
            <th>Batch</th>
            <th>Status</th>
            <th>Queue Position</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let run of runs">
            <td>{{ run.batchName }}</td>
            <td>{{ run.status }}</td>
            <td>{{ run.queuePosition }}</td>
          </tr>
        </tbody>
      </table>

      <div *ngIf="runs.length === 0">
        No batches in execution
      </div>
    </div>
  `,
  styles: [`
    .panel {
      padding: 12px;
    }
    table {
      width: 100%;
    }
  `]
})
export class StatusWindowComponent implements OnInit, OnDestroy {

  runs: BatchRunInfo[] = [];
  private sub?: Subscription;

  constructor(private batchService: BatchService) {}

  ngOnInit(): void {
    // ✅ Poll scheduler state every 2 seconds
    this.sub = interval(2000)
      .pipe(
        switchMap(() => this.batchService.getBatchRunQueue())
      )
      .subscribe({
        next: data => {
          this.runs = data ?? [];
        },
        error: err => console.error('Failed to load batch status', err)
      });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
