import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { BatchService, BatchRunInfo } from '../services/batch';

@Component({
  selector: 'app-batch-run',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './batch-run.html',
  styleUrls: ['./batch-run.css']
})
export class BatchRunComponent implements OnInit {

  availableBatches$!: Observable<string[]>;
  batchQueue$!: Observable<BatchRunInfo[]>;

  constructor(private batchService: BatchService) {}

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.availableBatches$ = this.batchService.getAllBatches();
    this.batchQueue$ = this.batchService.getBatchRunQueue();
  }

  addToQueue(name: string): void {
    this.batchService.addBatchToRunQueue(name)
      .subscribe(() => this.batchQueue$ =
        this.batchService.getBatchRunQueue());
  }
}
