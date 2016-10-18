import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DynamicFormComponent } from './dynamic-form/dynamic-form.component';
import { DynamicInputComponent } from './dynamic-input/dynamic-input.component';

@NgModule({
    imports: [CommonModule],
    declarations: [DynamicFormComponent, DynamicInputComponent],
    exports: [DynamicFormComponent, DynamicInputComponent]
})
export class DynamicFormsModule { }
