import { NgModule } from '@angular/core';

import { LogInComponent } from './log-in.component';
import { LogInRouting } from './log-in.routing';
import { DynamicFormsModule } from '../shared/dynamic-forms/dynamic-forms.module';
import { DynamicFormsResolve } from '../shared/dynamic-forms/dynamic-forms-resolve';

@NgModule({
    imports: [
        LogInRouting,
        DynamicFormsModule
    ],
    providers: [
        DynamicFormsResolve
    ],
    declarations: [
        LogInComponent
    ]
})
export class LogInModule { }
