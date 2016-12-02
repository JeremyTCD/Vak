import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { LogInComponent } from './log-in.component';
import { TwoFactorAuthComponent } from './two-factor-auth/two-factor-auth.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { LogInRouting } from './log-in.routing';
import { DynamicFormsModule } from '../shared/dynamic-forms/dynamic-forms.module';
import { DynamicFormGuard } from '../shared/dynamic-forms/dynamic-form/dynamic-form.guard';

@NgModule({
    imports: [
        LogInRouting,
        DynamicFormsModule,
        CommonModule
    ],
    providers: [
        DynamicFormGuard
    ],
    declarations: [
        LogInComponent,
        TwoFactorAuthComponent,
        ForgotPasswordComponent,
        ResetPasswordComponent
    ]
})
export class LogInModule { }
