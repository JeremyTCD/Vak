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
import { DynamicFormGuard } from '../shared/dynamic-forms/dynamic-form/dynamic-form.guard';
import { DynamicFormsModule } from '../shared/dynamic-forms/dynamic-forms.module';
import { VerifyEmailGuard } from './verify-email/verify-email.guard';

@NgModule({
    imports: [
        ManageAccountRouting,
        CommonModule,
        DynamicFormsModule
    ],
    providers: [
        ManageAccountGuard,
        DynamicFormGuard,
        VerifyEmailGuard
    ],
    declarations: [
        VerifyEmailComponent,
        ManageAccountComponent,
        ChangeAltEmailComponent,
        ChangeEmailComponent,
        ChangeDisplayNameComponent,
        ChangePasswordComponent
    ]
})
export class ManageAccountModule { }
