import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

const appRoutes: Routes = [
    { path: ``, pathMatch: `full`, redirectTo: `/home` },
    { path: `home`, loadChildren: `vak-app/home/home.module#HomeModule` },
    { path: `signup`, loadChildren: `vak-app/sign-up/sign-up.module#SignUpModule` },
    { path: `error`, loadChildren: `vak-app/error/error.module#ErrorModule` }
];

export const appRoutingProviders: any[] = [];

export const AppRouting: ModuleWithProviders = RouterModule.forRoot(appRoutes);

