import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { BatchService } from '../services/batch';
import { skip, distinctUntilChanged } from 'rxjs/operators';

/* Angular Material (header only) */
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

interface Method {
  methodId: string;
  methodName: string;
}

@Component({
  selector: 'app-batch-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './batch-create.html',
  styleUrls: ['./batch-create.css']
})
export class BatchCreateComponent implements OnInit {

  form!: FormGroup;
  existingBatches: string[] = [];
  methods: Method[] = [];

  constructor(
    private fb: FormBuilder,
    private batchService: BatchService
  ) {}

  ngOnInit(): void {
    console.log('✅ BatchCreateComponent initialized');

    this.form = this.fb.group({
      batchName: ['', Validators.required],
      samples: this.fb.array([])
    });

    this.loadBatches();
    this.loadMethods();
  }

  /* =========================
     Getters
     ========================= */
  get samples(): FormArray {
    return this.form.get('samples') as FormArray;
  }

  /* =========================
     Load master data
     ========================= */

  loadBatches(): void {
    this.batchService.getAllBatches().subscribe(b => {
      console.log('✅ Batches loaded:', b);
      this.existingBatches = b;
    });
  }

  loadMethods(): void {
    this.batchService.getMethods().subscribe(raw => {
      console.log('✅ Raw methods from backend:', raw);

      // ✅ FIX: map backend shape → UI shape
      this.methods = raw.map((m: any) => ({
        methodId: m.methodId ?? m.id,
        methodName: m.methodName ?? m.name
      }));

      console.log('✅ Methods mapped for UI:', this.methods);

      // Ensure at least one row exists
      if (this.samples.length === 0) {
        this.addSample();
      }
    });
  }

  /* =========================
     Batch selection
     ========================= */

  onBatchSelected(batchName: string): void {
    console.log('🟢 Batch selected:', batchName);

    if (!batchName) {
      this.resetForm();
      return;
    }

    this.batchService.getBatch(batchName).subscribe(batch => {
      this.form.patchValue({ batchName: batch.batchName });
      this.samples.clear();

      batch.samples.forEach((s: any, i: number) => {
        const group = this.createSampleRow(s);
        this.samples.push(group);
        this.watchMethodChange(group, i);
      });
    });
  }

  /* =========================
     Samples
     ========================= */

  createSampleRow(sample?: any): FormGroup {
    return this.fb.group({
      sampleName: [sample?.sampleName || '', Validators.required],
      injectionVolume: [sample?.injectionVolume || 10],
      methodId: [sample?.methodId || '', Validators.required]
    });
  }

  addSample(): void {
    const group = this.createSampleRow();
    this.samples.push(group);
    this.watchMethodChange(group, this.samples.length - 1);
  }

  resetForm(): void {
    this.form.reset({ batchName: '' });
    this.samples.clear();
    this.addSample();
  }

  /* =========================
     ✅ Method change tracking
     ========================= */

  private watchMethodChange(group: FormGroup, index: number): void {
    group.get('methodId')?.valueChanges
      .pipe(
        skip(1),                 // ignore initial value
        distinctUntilChanged()   // real user changes only
      )
      .subscribe(value => {
        console.log(`✅ Method changed in row ${index}:`, value);
      });
  }

  /* =========================
     Save
     ========================= */

  save(): void {
    console.log('💾 Saving batch:', this.form.value);

    if (this.form.invalid) return;

    this.batchService.saveBatch(this.form.value).subscribe(() => {
      alert('Batch saved successfully');
      this.resetForm();
    });
  }
}
