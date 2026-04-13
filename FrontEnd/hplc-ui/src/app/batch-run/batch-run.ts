import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import {
  BatchService,
  Batch,
  BatchRunInfo,
  SampleExecutionInfo,
  MsStatus
} from '../services/batch';

@Component({
  selector: 'app-batch-run',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './batch-run.html',
  styleUrls: ['./batch-run.css']
})
export class BatchRunComponent implements OnInit {

  availableBatches: string[] = [];
  runQueue: BatchRunInfo[] = [];

  selectedRun?: BatchRunInfo;
  msStatus?: MsStatus;
  isEnqueuing = false;
  isStarting = false;

  constructor(
    private batchService: BatchService,
    private cdr: ChangeDetectorRef
  ) {}

  /* =========================
     Lifecycle
     ========================= */

  ngOnInit(): void {
    this.loadAvailableBatches();
    this.loadRunQueue();

    // ✅ Auto-refresh while a batch is running (sample status updates)
    interval(1000).subscribe(() => {
      // if (this.runQueue.some(r => r.status === 'Running')) {
        this.loadRunQueue();
        this.loadMsStatus(); 
      //}
    });
  }

  /* =========================
     Available batches
     ========================= */

  loadAvailableBatches(): void {
    this.batchService.getAllBatches().subscribe({
      next: batches => {
        this.availableBatches = batches ?? [];
        this.cdr.detectChanges();
      },
      error: err => console.error(err)
    });
  }
  loadMsStatus(): void {
  this.batchService.getMsStatus().subscribe(status => {
    this.msStatus = status;
    this.cdr.detectChanges();
  });
  }
  addToQueue(batchName: string): void {
    if (this.isEnqueuing) return;
    this.isEnqueuing = true;

    this.batchService.getBatch(batchName).pipe(
      switchMap((batch: Batch) => this.batchService.enqueueBatch(batch)),
      switchMap(() => this.batchService.getBatchRunQueue())
    ).subscribe({
      next: queue => {
        this.runQueue = queue ?? [];
        this.selectedRun = this.runQueue[0];
        this.isEnqueuing = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.isEnqueuing = false;
        console.error(err);
      }
    });
  }

  /* =========================
     Run Queue
     ========================= */

  loadRunQueue(): void {
    this.batchService.getBatchRunQueue().subscribe({
      next: queue => {
        this.runQueue = queue ?? [];

        // ✅ Select running batch or first batch
        this.selectedRun =
          this.runQueue.find(r => r.status === 'Running')
          ?? this.runQueue[0];

        this.cdr.detectChanges();
      },
      error: err => console.error(err)
    });
  }

  startBatch(batchName: string): void {
    if (this.isStarting) return;
    this.isStarting = true;

    this.batchService.startBatch(batchName).subscribe({
      next: () => {
        this.isStarting = false;
        this.loadRunQueue();
      },
      error: err => {
        this.isStarting = false;
        console.error(err);
      }
    });
  }

  clearQueue(): void {
    this.batchService.clearRunQueue().pipe(
      switchMap(() => this.batchService.getBatchRunQueue())
    ).subscribe(queue => {
      this.runQueue = queue ?? [];
      this.selectedRun = undefined;
      this.cdr.detectChanges();
    });
  }

  /* =========================
     UI helpers
     ========================= */

  canRun(run: BatchRunInfo): boolean {
    return run.status === 'Queued' && !this.isStarting;
  }

  trackBatch(_: number, run: BatchRunInfo) {
    return run.batchName;
  }
}