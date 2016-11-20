import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';

const appRoutes: Routes = [
    { path: 'signup', loadChildren: 'app/sign-up/sign-up.module#SignUpModule' },
    { path: 'login', loadChildren: 'app/log-in/log-in.module#LogInModule' },
    { path: 'error', loadChildren: 'app/error/error.module#ErrorModule' },
    { path: '', pathMatch: 'full', redirectTo: '/home' },
    // TODO page does not exist component
    { path: '**', redirectTo: '/home' }
];

export const appRoutingProviders: any[] = [];

export const AppRouting: ModuleWithProviders = RouterModule.forRoot(appRoutes);

