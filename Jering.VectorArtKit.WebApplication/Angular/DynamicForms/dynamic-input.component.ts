import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { DynamicInput } from './dynamic-input-base';

@Component({
    selector: 'dynamic-input',
    templateUrl: 'dynamic-input.html'
})
export class DynamicInputComponent {
    @Input() dynamicInput: DynamicInput<any>;
    @Input() parentFormGroup: FormGroup;

    get isValid() { return this.parentFormGroup.controls[this.dynamicInput.name].valid; }
}
