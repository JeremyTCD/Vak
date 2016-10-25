import { Component, Input, Output, OnInit, EventEmitter } from '@angular/core';
import { Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';

import { DynamicForm } from './dynamic-form';
import { DynamicFormsService } from '../dynamic-forms.service';

@Component({
    selector: 'dynamic-form',
    templateUrl: 'dynamic-form.component.html'
})
export class DynamicFormComponent implements OnInit {
    @Input() formModelName: string;
    @Input() formSubmitUrl: string;
    @Output() submitSuccess = new EventEmitter<Response>();
    @Output() submitError = new EventEmitter<Response | any>();

    // create resolve guard to this does not need to be initialized
    dynamicForm: DynamicForm = new DynamicForm([], null);

    constructor(private _dynamicFormsService: DynamicFormsService) { }

    /**
     * Retrieves and sets dynamicForm
     */
    ngOnInit(): void {
        this._dynamicFormsService
            .getDynamicForm(this.formModelName)
            .subscribe(
            dynamicForm => {
                this.dynamicForm = dynamicForm;
            },
            // TODO - handle error
            error => alert('error: ' + error)
            );
    }

    /**
     * Sends json representation of dynamicForm values if form is valid
     */
    onSubmit(event: Event): void {
        event.preventDefault();

        if (this.dynamicForm.onSubmit()) {
            this.
                _dynamicFormsService.
                submitDynamicForm(this.formSubmitUrl, this.dynamicForm).
                subscribe(
                    (response: Response) => this.submitSuccess.emit(response),
                    (error: Response | any) => this.submitError.emit(error)
                );
        }
    }
}
