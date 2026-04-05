import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StatusWindowComponent } from './status-window';

describe('StatusWindowComponent', () => {
  let component: StatusWindowComponent;
  let fixture: ComponentFixture<StatusWindowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatusWindowComponent] // ✅ standalone component
    }).compileComponents();

    fixture = TestBed.createComponent(StatusWindowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
``