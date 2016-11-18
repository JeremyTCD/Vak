import { NgModule } from '@angular/core';

import { SignUpComponent } from './sign-up.component';
import { SignUpRouting } from './sign-up.routing';
import { DynamicFormsModule } from '../shared/dynamic-forms/dynamic-forms.module';
import { DynamicFormsResolve } from '../shared/dynamic-forms/dynamic-forms-resolve';

@NgModule({
    imports: [
        SignUpRouting,
        DynamicFormsModule
    ],
    providers: [
        DynamicFormsResolve
    ],
    declarations: [
        SignUpComponent
    ]
})
export class SignUpModule { }
