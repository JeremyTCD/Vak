import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ManageAccountComponent } from './manage-account.component';
import { ManageAccountGuard } from './manage-account.guard';
import { AuthGuard } from '../shared/auth.guard';
import { ChangeAltEmailComponent } from './change-alt-email/change-alt-email.component';
import { ChangeEmailComponent } from './change-email/change-email.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { ChangeDisplayNameComponent } from './change-display-name/change-display-name.component';
import { VerifyEmailComponent } from './verify-email/verify-email.component';
import { VerifyEmailGuard } from './verify-email/verify-email.guard';
import { DynamicFormGuard } from '../shared/dynamic-forms/dynamic-form/dynamic-form.guard';

const manageAccountRoutes: Routes = [
    {
        path: '',
        canActivateChild: [AuthGuard],
        children: [
            {
                path: '',
                component: ManageAccountComponent,
                resolve: {
                    responseModel: ManageAccountGuard
                }
            },
            {
                path: 'verify-email',
                component: VerifyEmailComponent,
                resolve: {
                    responseModel: VerifyEmailGuard
                }
            },
            {
                path: 'change-alt-email',
                component: ChangeAltEmailComponent,
                data: {
                    formModelName: ChangeAltEmailComponent.formModelName,
                    formSubmitRelativeUrl: ChangeAltEmailComponent.formSubmitRelativeUrl
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
            },
            {
                path: 'change-password',
                component: ChangePasswordComponent,
                data: {
                    formModelName: ChangePasswordComponent.formModelName,
                    formSubmitRelativeUrl: ChangePasswordComponent.formSubmitRelativeUrl
                },
                resolve: {
                    dynamicForm: DynamicFormGuard
                }
            }
        ]
    }
];

export const ManageAccountRouting: ModuleWithProviders = RouterModule.forChild(manageAccountRoutes);
