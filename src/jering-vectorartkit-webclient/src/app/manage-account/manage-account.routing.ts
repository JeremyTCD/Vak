import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ManageAccountComponent } from './manage-account.component';
import { ManageAccountGuard } from './manage-account.guard';
import { AuthenticationGuard } from '../shared/authentication.guard';
import { ChangeAlternativeEmailComponent } from './change-alternative-email/change-alternative-email.component';
import { DynamicFormGuard } from '../shared/dynamic-forms/dynamic-form/dynamic-form.guard';

const manageAccountRoutes: Routes = [
    {
        path: '',
        component: ManageAccountComponent,
        canActivate: [AuthenticationGuard],
        resolve: {
            responseModel: ManageAccountGuard
        }
    },
    {
        path: 'change-alternative-email',
        component: ChangeAlternativeEmailComponent,
        data: {
            formModelName: ChangeAlternativeEmailComponent.formModelName,
            formSubmitRelativeUrl: ChangeAlternativeEmailComponent.formSubmitRelativeUrl
        },
        canActivate: [AuthenticationGuard],
        resolve: {
            dynamicForm: DynamicFormGuard
        }
    }
];

export const ManageAccountRouting: ModuleWithProviders = RouterModule.forChild(manageAccountRoutes);
