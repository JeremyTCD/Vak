import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DynamicFormComponent } from './dynamic-form/dynamic-form.component';
import { DynamicControlComponent } from './dynamic-control/dynamic-control.component';
import { DynamicFormsService } from './dynamic-forms.service';

@NgModule({
    imports: [CommonModule],
    declarations: [DynamicFormComponent, DynamicControlComponent],
    providers: [ DynamicFormsService ],
    exports: [DynamicFormComponent, DynamicControlComponent]
})
export class DynamicFormsModule { }
