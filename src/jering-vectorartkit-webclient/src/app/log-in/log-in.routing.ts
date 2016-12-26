import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { LogInComponent } from './log-in.component';
import { TwoFactorAuthComponent } from './two-factor-auth/two-factor-auth.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { DynamicFormGuard } from 'app/shared/dynamic-forms/dynamic-form/dynamic-form.guard';
import { AppSegments } from 'app/app.segments';

const logInRoutes: Routes = [
    {
        path: '',
        component: LogInComponent,
        data: {
            requestModelName: LogInComponent.requestModelName,
            formSubmitRelativeUrl: LogInComponent.formSubmitRelativeUrl,
            getAfToken: true
        },
        resolve: {
            dynamicForm: DynamicFormGuard
        }
    },
    {
        path: AppSegments.twoFactorAuthSegment,
        component: TwoFactorAuthComponent,
        data: {
            requestModelName: TwoFactorAuthComponent.requestModelName,
            formSubmitRelativeUrl: TwoFactorAuthComponent.formSubmitRelativeUrl
        },
        resolve: {
            dynamicForm: DynamicFormGuard
        }
    },
    {
        path: AppSegments.forgotPasswordSegment,
        component: ForgotPasswordComponent,
        data: {
            requestModelName: ForgotPasswordComponent.requestModelName,
            formSubmitRelativeUrl: ForgotPasswordComponent.formSubmitRelativeUrl,
            getAfToken: true
        },
        resolve: {
            dynamicForm: DynamicFormGuard
        }
    },
    {
        path: AppSegments.resetPasswordSegment,
        component: ResetPasswordComponent,
        data: {
            requestModelName: ResetPasswordComponent.requestModelName,
            formSubmitRelativeUrl: ResetPasswordComponent.formSubmitRelativeUrl,
            getAfToken: true
        },
        resolve: {
            dynamicForm: DynamicFormGuard
        }
    },
];

export const LogInRouting: ModuleWithProviders = RouterModule.forChild(logInRoutes);
