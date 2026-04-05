import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BatchCreate } from './batch-create';

describe('BatchCreate', () => {
  let component: BatchCreate;
  let fixture: ComponentFixture<BatchCreate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BatchCreate],
    }).compileComponents();

    fixture = TestBed.createComponent(BatchCreate);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
