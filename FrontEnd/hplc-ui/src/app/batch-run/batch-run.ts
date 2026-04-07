import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';

import { BatchRunService } from '../services/batch-run.service';
import { BatchRunInfo } from '../models/batch-run-info.model';
import { SampleExecutionInfo } from '../models/sample-execution.model';

@Component({
  selector: 'app-batch-run',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './batch-run.html',
  styleUrls: ['./batch-run.css']
})
export class BatchRunComponent implements OnInit {

  availableBatches$!: Observable<string[]>;
  queue$!: Observable<BatchRunInfo[]>;
  execution$?: Observable<SampleExecutionInfo[]>;

  constructor(private service: BatchRunService) {}

  ngOnInit(): void {
    this.availableBatches$ = this.service.getAllBatches();
    this.queue$ = this.service.queue$;
    this.service.loadQueue();
  }

  addToQueue(batchName: string): void {
    this.service.enqueue(batchName).subscribe();
  }

  run(batchName: string): void {
    this.service.start(batchName).subscribe(() => {
      this.execution$ = this.service.getExecution();
    });
  }

  delete(batchName: string): void {
    this.service.remove(batchName).subscribe();
  }

  clear(): void {
    this.service.clear().subscribe(() => {
      this.execution$ = undefined;
    });
  }
}
