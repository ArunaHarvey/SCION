import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { BatchService, BatchRunInfo } from '../services/batch';

@Component({
  selector: 'app-status-window',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status-window.html'
})
export class StatusWindowComponent implements OnInit {

  runs$!: Observable<BatchRunInfo[]>;

  constructor(private batchService: BatchService) {}

  ngOnInit(): void {
    this.loadStatus();
  }

  loadStatus(): void {
    this.runs$ = this.batchService.getBatchRunQueue();
  }
}