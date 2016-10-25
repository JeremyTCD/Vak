import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ErrorComponent } from './error.component';

const errorRoutes: Routes = [
    { path: 'error/:errorMessage', component: ErrorComponent }
];

export const ErrorRouting: ModuleWithProviders = RouterModule.forChild(errorRoutes);