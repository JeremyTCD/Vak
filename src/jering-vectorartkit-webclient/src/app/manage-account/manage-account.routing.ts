import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ManageAccountComponent } from './manage-account.component';
import { ManageAccountGuard } from './manage-account.guard';
import { ChangeAltEmailComponent } from './change-alt-email/change-alt-email.component';
import { ChangeEmailComponent } from './change-email/change-email.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { ChangeDisplayNameComponent } from './change-display-name/change-display-name.component';
import { VerifyEmailComponent } from './verify-email/verify-email.component';
import { VerifyEmailGuard } from './verify-email/verify-email.guard';
import { VerifyAltEmailComponent } from './verify-alt-email/verify-alt-email.component';
import { VerifyAltEmailGuard } from './verify-alt-email/verify-alt-email.guard';
import { TwoFactorVerifyEmailComponent } from './two-factor-verify-email/two-factor-verify-email.component';

import { AuthGuard } from 'app/shared/auth.guard';
import { DynamicFormGuard } from 'app/shared/dynamic-forms/dynamic-form/dynamic-form.guard';
import { AppSegments } from 'app/app.segments';

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
                path: AppSegments.verifyEmailSegment,
                component: VerifyEmailComponent,
                resolve: {
                    responseModel: VerifyEmailGuard
                }
            },
            {
                path: AppSegments.verifyAltEmailSegment,
                component: VerifyAltEmailComponent,
                resolve: {
                    responseModel: VerifyAltEmailGuard
                }
            },
            {
                path: AppSegments.twoFactorVerifyEmailSegment,
                component: TwoFactorVerifyEmailComponent,
                data: {
                    requestModelName: TwoFactorVerifyEmailComponent.requestModelName,
                    formSubmitRelativeUrl: TwoFactorVerifyEmailComponent.formSubmitRelativeUrl
                },
                resolve: {
                    dynamicForm: DynamicFormGuard
                }
            },
            {
                path: AppSegments.changeAltEmailSegment,
                component: ChangeAltEmailComponent,
                data: {
                    requestModelName: ChangeAltEmailComponent.requestModelName,
                    formSubmitRelativeUrl: ChangeAltEmailComponent.formSubmitRelativeUrl
                },
                resolve: {
                    dynamicForm: DynamicFormGuard
                }
            },
            {
                path: AppSegments.changeEmailSegment,
                component: ChangeEmailComponent,
                data: {
                    requestModelName: ChangeEmailComponent.requestModelName,
                    formSubmitRelativeUrl: ChangeEmailComponent.formSubmitRelativeUrl
                },
                resolve: {
                    dynamicForm: DynamicFormGuard
                }
            },
            {
                path: AppSegments.changeDisplayNameSegment,
                component: ChangeDisplayNameComponent,
                data: {
                    requestModelName: ChangeDisplayNameComponent.requestModelName,
                    formSubmitRelativeUrl: ChangeDisplayNameComponent.formSubmitRelativeUrl
                },
                resolve: {
                    dynamicForm: DynamicFormGuard
                }
            },
            {
                path: AppSegments.changePasswordSegment,
                component: ChangePasswordComponent,
                data: {
                    requestModelName: ChangePasswordComponent.requestModelName,
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
