import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ManageAccountComponent } from './manage-account.component';
import { ManageAccountGuard } from './manage-account.guard';
import { AuthenticationGuard } from '../shared/authentication.guard';
import { ChangeAlternativeEmailComponent } from './change-alternative-email/change-alternative-email.component';
import { ChangeEmailComponent } from './change-email/change-email.component';
import { ChangeDisplayNameComponent } from './change-display-name/change-display-name.component';
import { DynamicFormGuard } from '../shared/dynamic-forms/dynamic-form/dynamic-form.guard';

const manageAccountRoutes: Routes = [
    {
        path: '',
        canActivateChild: [AuthenticationGuard],
        children: [
            {
                path: '',
                component: ManageAccountComponent,
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
                resolve: {
                    dynamicForm: DynamicFormGuard
                }
            },
            {
                path: 'change-email',
                component: ChangeEmailComponent,
                data: {
                    formModelName: ChangeEmailComponent.formModelName,
                    formSubmitRelativeUrl: ChangeEmailComponent.formSubmitRelativeUrl
                },
                resolve: {
                    dynamicForm: DynamicFormGuard
                }
            },
            {
                path: 'change-display-name',
                component: ChangeDisplayNameComponent,
                data: {
                    formModelName: ChangeDisplayNameComponent.formModelName,
                    formSubmitRelativeUrl: ChangeDisplayNameComponent.formSubmitRelativeUrl
                },
                resolve: {
                    dynamicForm: DynamicFormGuard
                }
            }
        ]
    }
];

export const ManageAccountRouting: ModuleWithProviders = RouterModule.forChild(manageAccountRoutes);
