import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';

/* =========================
   Instrument Models
   ========================= */

/**
 * LC runtime states
 * This is NOT batch state
 */
export type LcState =
  | 'Idle'
  | 'Injecting'
  | 'Running'
  | 'WaitingForMS';

/**
 * One row per LC (HPLC1–HPLC4)
 */
export interface LcStatus {
  lcId: string;               // HPLC1, HPLC2, ...
  state: LcState;             // Idle / Injecting / Running / WaitingForMS
  currentSample?: string;     // Assay_S01
}

/**
 * There is ONLY ONE MS
 */
export interface MsStatus {
  isAcquiring: boolean;
  activeLcId?: string;        // Which LC currently owns MS
  batchName?: string;         // Active batch name
}

/* =========================
   Service
   ========================= */

@Injectable({
  providedIn: 'root'   // ✅ Singleton, system‑wide truth
})
export class InstrumentStatusService {

  /**
   * Use this when backend is ready:
   * private readonly api = 'https://localhost:7260/api/status';
   */
  constructor(private http: HttpClient) {}

  /* ===========================================================
     POC MODE – DUMMY DATA (NO BACKEND REQUIRED)
     =========================================================== */

  /**
   * Returns current state of all LCs
   * Drives the "Instrument Status" window
   */
  getLcStatus(): Observable<LcStatus[]> {
    return of([
      {
        lcId: 'HPLC1',
        state: 'Running',
        currentSample: 'Assay_S01'
      },
      {
        lcId: 'HPLC2',
        state: 'Injecting',
        currentSample: 'Assay_S02'
      },
      {
        lcId: 'HPLC3',
        state: 'Idle'
      },
      {
        lcId: 'HPLC4',
        state: 'Idle'
      }
    ]);
  }

  /**
   * Returns MS (Mass Spectrometer) status
   * MS MUST be globally exclusive
   */
  getMsStatus(): Observable<MsStatus> {
    return of({
      isAcquiring: true,
      activeLcId: 'HPLC1',
      batchName: 'Batch_Assay_Run'
    });
  }

  /* ===========================================================
     REAL BACKEND MODE (enable later)
     =========================================================== */

  /*
  getLcStatus(): Observable<LcStatus[]> {
    return this.http.get<LcStatus[]>(
      `${this.api}/lcs`
    );
  }

  getMsStatus(): Observable<MsStatus> {
    return this.http.get<MsStatus>(
      `${this.api}/ms`
    );
  }
  */
}
