import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { LogInComponent } from './log-in.component';
import { TwoFactorAuthComponent } from './two-factor-auth/two-factor-auth.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { DynamicFormGuard } from '../shared/dynamic-forms/dynamic-form/dynamic-form.guard';

const logInRoutes: Routes = [
    {
        path: '',
        component: LogInComponent,
        data: {
            formModelName: LogInComponent.formModelName,
            formSubmitRelativeUrl: LogInComponent.formSubmitRelativeUrl,
            getAfToken: true
        },
        resolve: {
            dynamicForm: DynamicFormGuard
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
            dynamicForm: DynamicFormGuard
        }
    },
    {
        path: 'forgot-password',
        component: ForgotPasswordComponent,
        data: {
            formModelName: ForgotPasswordComponent.formModelName,
            formSubmitRelativeUrl: ForgotPasswordComponent.formSubmitRelativeUrl,
            getAfToken: true
        },
        resolve: {
            dynamicForm: DynamicFormGuard
        }
    },
    {
        path: 'reset-password',
        component: ResetPasswordComponent,
        data: {
            formModelName: ResetPasswordComponent.formModelName,
            formSubmitRelativeUrl: ResetPasswordComponent.formSubmitRelativeUrl,
            getAfToken: true
        },
        resolve: {
            dynamicForm: DynamicFormGuard
        }
    },
];

export const LogInRouting: ModuleWithProviders = RouterModule.forChild(logInRoutes);
