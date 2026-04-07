import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';   // ✅ REQUIRED
import { Subscription, interval } from 'rxjs';

import {
  InstrumentStatusService,
  LcStatus,
  MsStatus
} from '../services/instrument-status.service';

@Component({
  selector: 'app-instrument-status',
  standalone: true,               // ✅ Standalone component
  imports: [CommonModule],        // ✅ THIS FIXES NG8103
  templateUrl: './instrument-status.component.html',
  styleUrls: ['./instrument-status.component.scss']
})
export class InstrumentStatusComponent
  implements OnInit, OnDestroy {

  lcs: LcStatus[] = [];
  ms?: MsStatus;

  private timerSub?: Subscription;

  constructor(
    private instrumentStatusService: InstrumentStatusService
  ) {}

  ngOnInit(): void {
    this.loadStatus();
    this.timerSub = interval(2000).subscribe(() =>
      this.loadStatus()
    );
  }

  ngOnDestroy(): void {
    this.timerSub?.unsubscribe();
  }

  private loadStatus(): void {
    this.instrumentStatusService.getLcStatus()
      .subscribe(data => this.lcs = data);

    this.instrumentStatusService.getMsStatus()
      .subscribe(data => this.ms = data);
  }

  lcColor(state: string): string {
    switch (state) {
      case 'Running': return 'green';
      case 'Injecting': return 'orange';
      case 'WaitingForMS': return 'goldenrod';
      default: return 'gray';
    }
  }
}