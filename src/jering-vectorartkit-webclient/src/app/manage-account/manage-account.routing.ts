import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ManageAccountComponent } from './manage-account.component';
import { ManageAccountGuard } from './manage-account.guard';
import { AuthenticationGuard } from '../shared/authentication.guard';

const manageAccountRoutes: Routes = [
    {
        path: '',
        component: ManageAccountComponent,
        canActivate: [AuthenticationGuard],
        resolve: {
            responseModel: ManageAccountGuard
        }
    }
];

export const ManageAccountRouting: ModuleWithProviders = RouterModule.forChild(manageAccountRoutes);
