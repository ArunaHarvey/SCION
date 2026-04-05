import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BatchRunComponent } from './batch-run';

describe('BatchRunComponent', () => {
  let component: BatchRunComponent;
  let fixture: ComponentFixture<BatchRunComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [BatchRunComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BatchRunComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
