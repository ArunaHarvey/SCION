import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BatchCreateComponent } from './batch-create/batch-create';
import { BatchRunComponent } from './batch-run/batch-run';
import { StatusWindowComponent } from './status-window/status-window';

type TabType = 'create' | 'run' | 'status';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    BatchCreateComponent,
    BatchRunComponent,
    StatusWindowComponent
  ],
  template: `
    <!-- ✅ Top Navigation Bar -->
    <nav class="navbar">
      <span class="app-title">HPLC Batch Controller</span>

      <ul class="nav-menu">
        <li
          [class.active]="tab === 'create'"
          (click)="tab = 'create'">
          Batch Create
        </li>

        <li
          [class.active]="tab === 'run'"
          (click)="tab = 'run'">
          Batch Run
        </li>

        <li
          [class.active]="tab === 'status'"
          (click)="tab = 'status'">
          Status
        </li>
      </ul>
    </nav>

    <!-- ✅ Content Area -->
    <section class="content">
      <app-batch-create *ngIf="tab === 'create'"></app-batch-create>
      <app-batch-run *ngIf="tab === 'run'"></app-batch-run>
      <app-status-window *ngIf="tab === 'status'"></app-status-window>
    </section>
  `,
  styles: [`
    .navbar {
      height: 56px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 24px;
      background-color: #005cbb;
      color: white;
      box-shadow: 0 2px 6px rgba(0,0,0,0.15);
    }

    .app-title {
      font-size: 1rem;
      font-weight: 500;
    }

    .nav-menu {
      list-style: none;
      display: flex;
      gap: 20px;
      margin: 0;
      padding: 0;
    }

    .nav-menu li {
      cursor: pointer;
      padding: 6px 12px;
      border-radius: 4px;
      opacity: 0.85;
      transition: background-color .2s ease;
    }

    .nav-menu li:hover {
      background: rgba(255,255,255,0.15);
    }

    .nav-menu li.active {
      font-weight: 600;
      background: rgba(255,255,255,0.25);
      opacity: 1;
    }

    .content {
      padding: 24px;
    }
  `]
})
export class AppComponent {
  tab: TabType = 'create';   // ✅ default tab
}