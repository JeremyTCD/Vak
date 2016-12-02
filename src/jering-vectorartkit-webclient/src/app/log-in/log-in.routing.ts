import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { LogInComponent } from './log-in.component';
import { TwoFactorAuthComponent } from './two-factor-auth/two-factor-auth.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { DynamicFormsResolve } from '../shared/dynamic-forms/dynamic-forms-resolve';

const logInRoutes: Routes = [
    {
        path: '',
        component: LogInComponent,
        data: {
            formModelName: LogInComponent.formModelName,
            formSubmitRelativeUrl: LogInComponent.formSubmitRelativeUrl
        },
        resolve: {
            dynamicForm: DynamicFormsResolve
        }
    },
    {
        path: 'two-factor-auth',
        component: TwoFactorAuthComponent,
        data: {
            formModelName: TwoFactorAuthComponent.formModelName,
            formSubmitRelativeUrl: TwoFactorAuthComponent.formSubmitRelativeUrl
        },
        resolve: {
            dynamicForm: DynamicFormsResolve
        }
    },
    {
        path: 'forgot-password',
        component: ForgotPasswordComponent,
        data: {
            formModelName: ForgotPasswordComponent.formModelName,
            formSubmitRelativeUrl: ForgotPasswordComponent.formSubmitRelativeUrl
        },
        resolve: {
            dynamicForm: DynamicFormsResolve
        }
    },
    {
        path: 'reset-password',
        component: ResetPasswordComponent,
        data: {
            formModelName: ResetPasswordComponent.formModelName,
            formSubmitRelativeUrl: ResetPasswordComponent.formSubmitRelativeUrl
        },
        resolve: {
            dynamicForm: DynamicFormsResolve
        }
    },
];

export const LogInRouting: ModuleWithProviders = RouterModule.forChild(logInRoutes);
