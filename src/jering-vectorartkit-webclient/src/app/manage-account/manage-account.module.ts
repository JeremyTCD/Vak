import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ManageAccountComponent } from './manage-account.component';
import { ManageAccountRouting } from './manage-account.routing';
import { ManageAccountGuard } from './manage-account.guard';
import { ChangeAltEmailComponent } from './change-alt-email/change-alt-email.component';
import { ChangeEmailComponent } from './change-email/change-email.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { ChangeDisplayNameComponent } from './change-display-name/change-display-name.component';
import { VerifyEmailComponent } from './verify-email/verify-email.component';
import { DynamicFormGuard } from 'app/shared/dynamic-forms/dynamic-form/dynamic-form.guard';
import { DynamicFormsModule } from 'app/shared/dynamic-forms/dynamic-forms.module';
import { VerifyEmailGuard } from './verify-email/verify-email.guard';
import { VerifyAltEmailComponent } from './verify-alt-email/verify-alt-email.component';
import { VerifyAltEmailGuard } from './verify-alt-email/verify-alt-email.guard';
import { TwoFactorVerifyEmailComponent } from './two-factor-verify-email/two-factor-verify-email.component';

@NgModule({
    imports: [
        ManageAccountRouting,
        CommonModule,
        DynamicFormsModule
    ],
    providers: [
        ManageAccountGuard,
        DynamicFormGuard,
        VerifyEmailGuard,
        VerifyAltEmailGuard
    ],
    declarations: [
        VerifyAltEmailComponent,
        VerifyEmailComponent,
        ManageAccountComponent,
        ChangeAltEmailComponent,
        ChangeEmailComponent,
        ChangeDisplayNameComponent,
        ChangePasswordComponent,
        TwoFactorVerifyEmailComponent
    ]
})
export class ManageAccountModule { }
