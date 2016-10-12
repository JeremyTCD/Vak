import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { DynamicFormComponent } from './dynamic-form.component';
import { DynamicInputComponent } from './dynamic-input.component';

@NgModule({
    imports: [CommonModule, ReactiveFormsModule],
    declarations: [DynamicFormComponent, DynamicInputComponent],
    exports: [DynamicFormComponent, DynamicInputComponent]
})
export class DynamicFormsModule { }
