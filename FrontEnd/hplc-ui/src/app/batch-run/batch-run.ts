import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import {
  BatchService,
  BatchRunInfo,
  MsStatus,
  ChromStatus,
  ChromPoint
} from '../services/batch';

@Component({
  selector: 'app-batch-run',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './batch-run.html',
  styleUrls: ['./batch-run.css']
})
export class BatchRunComponent implements OnInit {

  /* =========================
     Available batches
     ========================= */
  availableBatches: string[] = [];
  activeMsSample: any | null = null;

  /* =========================
     Run queue
     ========================= */
  runQueue: BatchRunInfo[] = [];
  selectedRun?: BatchRunInfo;

  /* =========================
     Instrument status
     ========================= */
  msStatus?: MsStatus;
  chromMeta?: ChromStatus;

  /* =========================
     Chromatogram rendering
     ========================= */
  chromPoints: ChromPoint[] = [];
  svgWidth = 600;
  svgHeight = 300;

  /* =========================
     UI state
     ========================= */
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

    // Poll runtime status
    interval(1000).subscribe(() => {
      this.loadRunQueue();
      this.loadMsStatus();
      this.loadChromatograph();
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
      error: err => console.error('Failed to load batches', err)
    });
  }

  /** ✅ FIX: call enqueueBatchByName, NOT enqueueBatch */
  addToQueue(batchName: string): void {
    if (this.isEnqueuing) return;

    this.isEnqueuing = true;

    this.batchService.enqueueBatchByName(batchName).pipe(
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
        console.error('Failed to enqueue batch', err);
      }
    });
  }

  /* =========================
     Run queue
     ========================= */

loadRunQueue(): void {
  this.batchService.getBatchRunQueue().subscribe(queue => {

    const normalizedQueue =
      (queue ?? []).sort((a, b) => a.queuePosition - b.queuePosition);

    this.runQueue = normalizedQueue;

    const runningBatch =
      normalizedQueue.find(r => r.status === 'Running') ?? null;

    this.selectedRun =
      runningBatch
      ?? normalizedQueue[0]
      ?? null;

    this.activeMsSample =
      this.selectedRun?.samples.find(s => s.state === 'Acquiring') ?? null;

    // ✅ notify Angular that new data arrived
    this.cdr.markForCheck();
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
        console.error('Failed to start batch', err);
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

  canRun(run: BatchRunInfo): boolean {
    return run.status === 'Queued' && !this.isStarting;
  }

  trackBatch(_: number, run: BatchRunInfo): string {
    return run.batchName;
  }

  /* =========================
     MS status
     ========================= */

  loadMsStatus(): void {
    this.batchService.getMsStatus().subscribe(
      status => this.msStatus = status
    );
  }

  /* =========================
     Chromatograph
     ========================= */

  loadChromatograph(): void {
    this.batchService.getChromatographStatus()
      .subscribe((meta: ChromStatus) => {
        this.chromMeta = meta;

        if (meta.state === 'Running' && meta.batch && meta.sample) {
          this.batchService
            .getChromData(meta.batch, meta.sample)
            .subscribe((points: ChromPoint[]) => {
              this.chromPoints = points ?? [];
            });
        } else {
          this.chromPoints = [];
        }
      });
  }
  
  getSampleForLc(lc: number) {
  return this.selectedRun?.samples.find(s => s.assignedLC === lc);
}

getSampleNameForLc(lc: number): string {
  return this.getSampleForLc(lc)?.sampleName ?? 'Idle';
}

getSampleStateForLc(lc: number): string {
  return this.getSampleForLc(lc)?.state ?? 'Idle';
}
  

  /* =========================
     SVG helper
     ========================= */

  polyline(): string {
    if (!this.chromPoints.length) return '';

    const maxX = Math.max(...this.chromPoints.map(p => p.time));
    const maxY = Math.max(...this.chromPoints.map(p => p.intensity));

    return this.chromPoints.map(p => {
      const x = (p.time / maxX) * this.svgWidth;
      const y = this.svgHeight - (p.intensity / maxY) * this.svgHeight;
      return `${x},${y}`;
    }).join(' ');
  }
}