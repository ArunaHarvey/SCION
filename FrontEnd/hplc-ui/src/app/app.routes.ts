import { Routes } from '@angular/router';
import { BatchCreateComponent } from './batch-create/batch-create';
import { BatchRunComponent } from './batch-run/batch-run';
import { StatusWindowComponent } from './status-window/status-window';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'batch-create',
    pathMatch: 'full'
  },
  {
    path: 'batch-create',
    component: BatchCreateComponent
  },
  {
    path: 'batch-run',
    component: BatchRunComponent
  },
  {
    path: 'status',
    component: StatusWindowComponent
  }
];