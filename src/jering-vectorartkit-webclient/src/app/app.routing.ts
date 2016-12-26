import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';

import { AppSegments } from 'app/app.segments';
import { AppPaths } from 'app/app.paths';

const appRoutes: Routes = [
    { path: AppSegments.signUpSegment, loadChildren: 'app/sign-up/sign-up.module#SignUpModule' },
    { path: AppSegments.logInSegment, loadChildren: 'app/log-in/log-in.module#LogInModule' },
    { path: AppSegments.errorSegment, loadChildren: 'app/error/error.module#ErrorModule' },
    { path: AppSegments.manageAccountSegment, loadChildren: 'app/manage-account/manage-account.module#ManageAccountModule' },
    { path: '', pathMatch: 'full', redirectTo: AppPaths.homePath},
    // TODO page does not exist component
    { path: '**', redirectTo: AppPaths.homePath }
];

export const appRoutingProviders: any[] = [];

export const AppRouting: ModuleWithProviders = RouterModule.forRoot(appRoutes);

