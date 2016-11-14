import { Component, Input, Output, OnInit, OnDestroy, EventEmitter } from '@angular/core';
import { Router } from '@angular/router/index';
import { Response } from '@angular/http';
import { Subscription } from 'rxjs';

import { DynamicForm } from './dynamic-form';
import { DynamicFormsService } from '../dynamic-forms.service';
import { ErrorHandlerService } from '../../utility/error-handler.service';
import { Validity } from '../validity';

@Component({
    selector: 'dynamic-form',
    templateUrl: 'dynamic-form.component.html'
})
export class DynamicFormComponent implements OnInit, OnDestroy {
    @Input() formModelName: string;
    @Input() formSubmitUrl: string;
    @Output() submitSuccess = new EventEmitter<Response>();

    // create resolve guard to this does not need to be initialized
    dynamicForm: DynamicForm = new DynamicForm([], null);

    private _getDynamicFormSubscription: Subscription;
    private _submitDynamicFormSubscription: Subscription;

    constructor(private _dynamicFormsService: DynamicFormsService, private _router: Router) { }

    /**
     * Retrieves and sets dynamicForm
     */
    ngOnInit(): void {
        this._getDynamicFormSubscription = this._dynamicFormsService
            .getDynamicForm(this.formModelName)
            .subscribe(dynamicForm => {
                this.dynamicForm = dynamicForm;
            });
    }

    /**
     * Sends json representation of dynamicForm values if form is valid
     */
    onSubmit(event: any): void {
        event.preventDefault();

        // TODO disable resubmission or editing of fields until response arrives
        if (this.dynamicForm.onSubmit()) {
            this._submitDynamicFormSubscription = this.
                _dynamicFormsService.
                submitDynamicForm(this.formSubmitUrl, this.dynamicForm).
                subscribe(
                    data => this.submitSuccess.emit(data),
                    this.handleSubmitDynamicFormError
                );
        }
    }

    private handleSubmitDynamicFormError = (error: { [key: string]: string[] }): void => {
        for (let key of Object.keys(error)) {
            let dynamicControl = this.dynamicForm.getDynamicControl(key);
            if (dynamicControl) {
                dynamicControl.messages = error[key];
                dynamicControl.validity = Validity.invalid;
            }
        }
        this.dynamicForm.messages = [this.dynamicForm.message];
        this.dynamicForm.validity = Validity.invalid;
    }

    ngOnDestroy(): void {
        this.submitSuccess.unsubscribe();

        if (this._getDynamicFormSubscription) {
            this._getDynamicFormSubscription.unsubscribe();
        }

        if (this._submitDynamicFormSubscription) {
            this._submitDynamicFormSubscription.unsubscribe();
        }

        if (this.dynamicForm) {
            this.dynamicForm.dispose();
        }
    }
}
