import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { HomeComponent } from './home.component';

import { AppSegments } from 'app/app.segments';

const homeRoutes: Routes = [
    { path: AppSegments.homeSegment, component: HomeComponent }
];

export const HomeRouting: ModuleWithProviders = RouterModule.forChild(homeRoutes);
