import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';

import { SignUpComponent } from './sign-up.component';
import { SignUpRouting } from './sign-up.routing';
import { DynamicFormsModule } from '../shared/dynamic-forms/dynamic-forms.module';

@NgModule({
    imports: [
        HttpModule,
        SignUpRouting,
        DynamicFormsModule
    ],
    declarations: [
        SignUpComponent
    ]
})
export class SignUpModule { }
