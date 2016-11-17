import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DynamicFormComponent } from './dynamic-form/dynamic-form.component';
import { DynamicControlComponent } from './dynamic-control/dynamic-control.component';
import { DynamicFormsService } from './dynamic-forms.service';
import { ErrorHandlerService } from '../utility/error-handler.service';

@NgModule({
    imports: [CommonModule],
    declarations: [DynamicFormComponent, DynamicControlComponent],
    providers: [DynamicFormsService, ErrorHandlerService],
    exports: [DynamicFormComponent, DynamicControlComponent]
})
export class DynamicFormsModule { }
