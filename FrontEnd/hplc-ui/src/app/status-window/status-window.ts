import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, interval, startWith, switchMap } from 'rxjs';
import { BatchService } from '../services/batch';
import { BatchRunInfo } from  '../models/batch-run-info.model';
import { BatchRunSummary } from   '../models/batch-run-summary';

@Component({
  selector: 'app-status-window',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status-window.html',
  styleUrls: ['./status-window.css']
})
export class StatusWindowComponent implements OnInit {

  runs$!: Observable<BatchRunInfo[]>;
  summary$!: Observable<BatchRunSummary | null>;

  constructor(private batchService: BatchService) {}

  ngOnInit(): void {
    this.runs$ = interval(2000).pipe(
      startWith(0),
      switchMap(() => this.batchService.getBatchRunQueue())
    );

    this.summary$ = interval(5000).pipe(
      startWith(0),
      switchMap(() => this.batchService.getBatchRunSummary())
    );
  }
}