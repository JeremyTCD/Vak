import { NgModule } from '@angular/core';

import { SignUpComponent } from './sign-up.component';
import { SignUpRouting } from './sign-up.routing';
import { DynamicFormsModule } from 'app/shared/dynamic-forms/dynamic-forms.module';
import { DynamicFormGuard } from 'app/shared/dynamic-forms/dynamic-form/dynamic-form.guard';

@NgModule({
    imports: [
        SignUpRouting,
        DynamicFormsModule
    ],
    providers: [
        DynamicFormGuard
    ],
    declarations: [
        SignUpComponent
    ]
})
export class SignUpModule { }
