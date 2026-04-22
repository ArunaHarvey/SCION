import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval, Subscription } from 'rxjs';
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
export class BatchRunComponent implements OnInit, OnDestroy {

  /* =========================
     Available batches
     ========================= */
  availableBatches: string[] = [];
  

  /* =========================
     Run queue
     ========================= */
  runQueue: BatchRunInfo[] = [];
  selectedRun: BatchRunInfo | null = null;

  /* =========================
     Instrument / MS state
     ========================= */
  msStatus?: MsStatus;
  activeMsSample: any | null = null;

  /* =========================
     Chromatograph metadata
     ========================= */
  chromMeta?: ChromStatus;

  /* =========================
     Chromatogram (live)
     ========================= */
  chromPoints: ChromPoint[] = [];
  currentChromSample: string | null = null;
  chromTime = 0;

  svgWidth = 600;
  svgHeight = 300;

  /* =========================
     UI state
     ========================= */
  isEnqueuing = false;
  isStarting = false;
  
/* =========================
   Batch summary
   ========================= */
  batchSummary: any | null = null;

  /* =========================
     Polling subscriptions
     ========================= */
  private pollSub?: Subscription;
  private chromSub?: Subscription;

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
    this.loadMsStatus();

    // Execution & MS polling (authoritative)
    this.pollSub = interval(1000).subscribe(() => {
      this.loadRunQueue();
      this.loadMsStatus();
    });

    // Live chromatogram emission (visual)
    this.chromSub = interval(200).subscribe(() => {
      this.emitChromPoint();
    });
  }

  ngOnDestroy(): void {
    this.pollSub?.unsubscribe();
    this.chromSub?.unsubscribe();
  }

  /* =========================
     Available batches
     ========================= */

  loadAvailableBatches(): void {
    this.batchService.getAllBatches().subscribe({
      next: batches => {
        this.availableBatches = batches ?? [];
        
        // ✅ Load summary once batch is completed
        if (
          this.selectedRun &&
          this.selectedRun.status === 'Completed' &&
          !this.batchSummary
        ) {
          this.loadBatchSummary();
        }

        this.cdr.markForCheck();
      },
      error: err => console.error('Failed to load batches', err)
    });
  }

  addToQueue(batchName: string): void {
    if (this.isEnqueuing) return;

    this.isEnqueuing = true;

    this.batchService.enqueueBatchByName(batchName).pipe(
      switchMap(() => this.batchService.getBatchRunQueue())
    ).subscribe({
      next: queue => {
        this.runQueue = queue ?? [];
        this.selectedRun = this.runQueue[0] ?? null;
        this.isEnqueuing = false;
        this.cdr.markForCheck();
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

      // Reset chromatogram on MS sample switch
      const newSample = this.activeMsSample?.sampleName ?? null;
      if (newSample !== this.currentChromSample) {
        this.currentChromSample = newSample;
        this.chromPoints = [];
        this.chromTime = 0;
      }
      
              // ⭐⭐⭐ Load summary when batch finishes ⭐⭐⭐
        if (this.selectedRun?.status === 'Completed' && !this.batchSummary) {
          this.loadBatchSummary();
        }
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
      this.selectedRun = null;
      this.activeMsSample = null;
      this.chromPoints = [];
      this.chromTime = 0;
      this.currentChromSample = null;
      this.cdr.markForCheck();
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
    this.batchService.getMsStatus().subscribe(status => {
      this.msStatus = status;
      this.cdr.markForCheck();
    });
  }

  /* =========================
     Live chromatogram
     ========================= */

  emitChromPoint(): void {
    if (!this.activeMsSample) return;
    if (this.activeMsSample.state !== 'Acquiring') return;

    this.chromTime += 0.2;

    const point = this.generateChromPoint(
      this.chromTime,
      this.activeMsSample.sampleName
    );

    this.chromPoints = [...this.chromPoints, point];

    if (this.chromPoints.length > 400) {
      this.chromPoints = this.chromPoints.slice(-400);
    }

    this.cdr.markForCheck();
  }

  generateChromPoint(time: number, sampleName: string): ChromPoint {
    const seed = this.hashSample(sampleName);

    const baseline = 120 + (seed % 30);
    const noise = (Math.random() - 0.5) * 40;

    const apex = 35 + (seed % 10);
    const height = 6000 + (seed % 3000);
    const width = 8 + (seed % 4);

    const peak =
      height *
      Math.exp(-Math.pow(time - apex, 2) / (2 * width * width));

    return {
      time,
      intensity: Math.max(0, baseline + peak + noise)
    };
  }

  private hashSample(name: string): number {
    return Array.from(name)
      .reduce((acc, c) => acc + c.charCodeAt(0), 0);
  }

  /* =========================
     SVG helper
     ========================= */

  polyline(): string {
  if (!this.chromPoints.length) return '';

  // Use a fixed time scale (no rescaling)
  const pixelsPerSecond = 8; // adjust speed here

  return this.chromPoints.map(p => {
    const x = p.time * pixelsPerSecond;
    const y =
      this.svgHeight -
      (p.intensity / this.getMaxIntensity()) * this.svgHeight;

    return `${x},${y}`;
  }).join(' ');
}

getMaxIntensity(): number {
  return Math.max(1, ...this.chromPoints.map(p => p.intensity));
}
loadBatchSummary(): void {
  this.batchService.getBatchRunSummary().subscribe({
    next: summary => {
      this.batchSummary = summary;
      this.cdr.markForCheck();
    },
    error: err =>
      console.error('Failed to load batch summary', err)
  });
}
}
