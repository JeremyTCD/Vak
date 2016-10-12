import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import { SignUpComponent } from './sign-up.component';
import { SignUpRouting } from './sign-up.routing';
import { DynamicFormsModule } from '../dynamic-forms/dynamic-forms.module';

@NgModule({
    imports: [
        FormsModule,
        HttpModule,
        SignUpRouting,
        DynamicFormsModule
    ],
    declarations: [
        SignUpComponent
    ]
})
export class SignUpModule { }