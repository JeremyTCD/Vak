import { Component, Input, OnInit } from '@angular/core';

import { DynamicForm } from './dynamic-form';
import { DynamicInput } from '../dynamic-input/dynamic-input';
import { DynamicFormsService } from '../dynamic-forms.service';

@Component({
    selector: 'dynamic-form',
    templateUrl: 'dynamic-form.component.html',
    providers: [DynamicFormsService]
})
export class DynamicFormComponent implements OnInit {
    @Input() formModelName: string;

    dynamicForm: DynamicForm = new DynamicForm([]);
    payLoad = '';

    constructor(private _dynamicFormsService: DynamicFormsService) { }

    ngOnInit() {
        this._dynamicFormsService
            .getDynamicInputs(this.formModelName)
            .subscribe(
            dynamicInputs => {
                this.dynamicForm = this._dynamicFormsService.createDynamicForm(dynamicInputs);
            },
            error => console.log('error: ' + error)
            );
    }

    onSubmit() {
        //this.payLoad = JSON.stringify(this.dynamicForm.value);
    }
}
