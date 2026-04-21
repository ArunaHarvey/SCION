import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { interval, Subscription } from 'rxjs';

@Component({
  selector: 'app-status-window',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status-window.html',
  styleUrls: ['./status-window.css']
})
export class StatusWindowComponent implements OnInit, OnDestroy {

  ms: any = { busy: false };
  lcs: any[] = [];
  chrom: any[] = [];
  chromKey = 0;

  batchName = '';
  sampleName = '';

  sub?: Subscription;

  constructor(private http: HttpClient) {}

ngOnInit(): void {
  this.sub = interval(500).subscribe(() => {

    /* =========================
       MS STATUS
       ========================= */
    this.http.get<any>('/api/status/ms')
      .subscribe(ms => {
        console.log('[MS API RESPONSE]', ms);

        this.ms = ms;

        console.log('[MS STATE AFTER SET]', {
          busy: this.ms?.busy,
          activeLc: this.ms?.activeLc
        });
      });

    /* =========================
       LC STATUS
       ========================= */
    this.http.get<any[]>('/api/status/lcs')
      .subscribe(lcs => {
        console.log('[LC STATUS]', lcs);
        this.lcs = lcs;
      });

    /* =========================
       CHROMATOGRAM META
       ========================= */
    this.http.get<any>('/api/status/chromatogram-meta')
      .subscribe(meta => {
        console.log('[CHROM META RAW]', meta);

        if (!meta?.batchName || !meta?.sampleName) {
          console.log('[CHROM META] NULL or INCOMPLETE');
          return;
        }

        console.log('[CHROM META VALID]', {
          batchName: meta.batchName,
          sampleName: meta.sampleName
        });

        if (this.sampleName !== meta.sampleName) {
          console.log('[CHROM] Sample changed, clearing trace');
          this.chrom = [];
        }

        this.batchName = meta.batchName;
        this.sampleName = meta.sampleName;

        console.log('[CHROM ROUTE]', `/api/status/chromatogram/${this.batchName}/${this.sampleName}`);

        /* =========================
           CHROMATOGRAM DATA
           ========================= */
        this.http.get<any[]>(
          `/api/status/chromatogram/${this.batchName}/${this.sampleName}`
        ).subscribe(points => {

          console.log('[CHROM POINTS RECEIVED]', points?.length ?? 0);

          if (!points || points.length === 0) {
            return;
          }

          this.chrom = [...this.chrom, ...points];

          console.log('[CHROM TOTAL POINTS]', this.chrom.length);
        });
      });

    /* =========================
       FINAL RENDER SNAPSHOT
       ========================= */
    console.log('[RENDER STATE SNAPSHOT]', {
      msBusy: this.ms?.busy,
      activeLc: this.ms?.activeLc,
      batchName: this.batchName,
      sampleName: this.sampleName,
      chromPoints: this.chrom.length
    });

  });
}
  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  lcColor(state: string): string {
    if (state === 'Acquiring') return 'green';
    if (state === 'Injecting' || state === 'WaitingForMS') return 'yellow';
    return 'off';
  }
}