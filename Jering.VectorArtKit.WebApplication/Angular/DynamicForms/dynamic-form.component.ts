import { Component, Input, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { DynamicInput } from './dynamic-input-base';
import { DynamicFormsService } from './dynamic-forms.service';

@Component({
    selector: 'dynamic-form',
    templateUrl: 'dynamic-form.html',
    providers: [DynamicFormsService]
})
export class DynamicFormComponent implements OnInit {
    @Input() formModelName: string;

    dynamicInputs: DynamicInput<any>[];
    // If null initially, the error "form group expects a FormGroup instance" is thrown.
    // Should use the resolve route guard to ensure that dynamicforminputs are loaded before rendering the page,
    // otherwise if a user clicks submit too quickly the server might receive empty data.
    formGroup: FormGroup = new FormGroup({});
    payLoad = '';

    constructor(private _dynamicFormsService: DynamicFormsService) { }

    ngOnInit() {
        this._dynamicFormsService
            .getDynamicInputs(this.formModelName)
            .subscribe(
                dynamicInputs => {
                    this.formGroup = this._dynamicFormsService.createFormGroup(dynamicInputs);
                    this.formGroup.statusChanges.subscribe(data => this.onStatusChange(data));
                    this.dynamicInputs = dynamicInputs;
                },
                error => console.log('error: ' + error)
            );
    }

    onStatusChange(data?: any) {
        for (let key in this.formGroup.controls) {
            let formControl = this.formGroup.controls[key];
            let dynamicInput = this.dynamicInputs.find(d => d.name === key);
            dynamicInput.errors = [];
            if (formControl.invalid && (formControl.dirty || formControl.touched)) {
                for (let error in formControl.errors) {
                    dynamicInput.errors.push(formControl.errors[error]);
                }
            } 
        }
    }

    onSubmit() {
        this.payLoad = JSON.stringify(this.formGroup.value);
    }
}
