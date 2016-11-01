import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';

import { SignUpComponent } from './sign-up.component';
import { SignUpRouting } from './sign-up.routing';
import { DynamicFormsModule } from '../shared/dynamic-forms/dynamic-forms.module';
import { ErrorHandlerService } from '../shared/utility/error-handler.service';

@NgModule({
    imports: [
        HttpModule,
        SignUpRouting,
        DynamicFormsModule
    ],
    providers: [
        ErrorHandlerService
    ],
    declarations: [
        SignUpComponent
    ]
})
export class SignUpModule { }
