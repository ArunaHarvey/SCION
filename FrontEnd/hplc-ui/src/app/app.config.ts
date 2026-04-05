import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';

import { BatchCreateComponent } from './batch-create/batch-create';
import { BatchRunComponent } from './batch-run/batch-run';
import { StatusWindowComponent } from './status-window/status-window';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    provideRouter([
      { path: 'create-batch', component: BatchCreateComponent },
      { path: 'batch-run', component: BatchRunComponent },
      { path: 'status', component: StatusWindowComponent },
      { path: '', redirectTo: 'create-batch', pathMatch: 'full' }
    ])
  ]
};