import { Component, Input, OnInit } from '@angular/core';

import { DynamicForm } from './dynamic-form';
import { DynamicFormsService } from '../dynamic-forms.service';

@Component({
    selector: 'dynamic-form',
    templateUrl: 'dynamic-form.component.html'
})
export class DynamicFormComponent implements OnInit {
    @Input() formModelName: string;

    dynamicForm: DynamicForm = new DynamicForm([]);

    constructor(private _dynamicFormsService: DynamicFormsService) { }

    /**
     * Retrieves and sets dynamicForm
     */
    ngOnInit() {
        this._dynamicFormsService
            .getDynamicForm(this.formModelName)
            .subscribe(
                dynamicForm => {
                    this.dynamicForm = dynamicForm;
                },
                error => console.log('error: ' + error)
            );
    }

    /**
     * Sends json representation of dynamicForm values
     */
    onSubmit(event: Event) {
        // let value = JSON.stringify(this.dynamicForm.value);
        event.preventDefault();
    }
}
