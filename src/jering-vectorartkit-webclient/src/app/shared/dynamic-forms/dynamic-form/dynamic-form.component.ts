import { Component, Input, Output, OnInit, OnDestroy, EventEmitter } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router/index';
import { Response } from '@angular/http';
import { Subscription } from 'rxjs';

import { DynamicForm } from './dynamic-form';
import { DynamicFormsService } from '../dynamic-forms.service';
import { ErrorResponseModel } from '../../response-models/error.response-model';
import { Validity } from '../validity';
import { Check } from '../../check';

@Component({
    selector: 'dynamic-form',
    templateUrl: 'dynamic-form.component.html'
})
export class DynamicFormComponent implements OnInit, OnDestroy {
    formSubmitRelativeUrl: string;
    @Output() submitSuccess = new EventEmitter<Response>();

    // create resolve guard to this does not need to be initialized
    dynamicForm: DynamicForm = new DynamicForm([], null, null);

    private _dataSubscription: Subscription;
    private _submitDynamicFormSubscription: Subscription;

    constructor(private _dynamicFormsService: DynamicFormsService,
        private _router: Router,
        private _activatedRoute: ActivatedRoute) { }

    /**
     * Retrieves and sets dynamicForm
     */
    ngOnInit(): void {
        this._dataSubscription = this.
            _activatedRoute.
            data.
            subscribe((data: { dynamicForm: DynamicForm, formSubmitRelativeUrl: string }) => {
                this.dynamicForm = data.dynamicForm;
                this.formSubmitRelativeUrl = data.formSubmitRelativeUrl;
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
                submitDynamicForm(this.formSubmitRelativeUrl, this.dynamicForm).
                subscribe(
                    responseModel => this.submitSuccess.emit(responseModel),
                    this.handleSubmitDynamicFormError
                );
        }
    }

    /**
     * Handle failed validation
     */
    private handleSubmitDynamicFormError = (error: ErrorResponseModel): void => {
        if (Check.isObject(error.modelState)) {
            // Object.keys throws TypeError if its argument is not an object
            for (let key of Object.keys(error.modelState)) {
                let dynamicControl = this.dynamicForm.getDynamicControl(key);
                if (dynamicControl) {
                    dynamicControl.messages = error.modelState[key];
                    dynamicControl.validity = Validity.invalid
                }
            }
        }

        this.dynamicForm.messages = [error.errorMessage || this.dynamicForm.message];
        this.dynamicForm.validity = Validity.invalid;
    }

    ngOnDestroy(): void {
        this.submitSuccess.unsubscribe();

        if (this._dataSubscription) {
            this._dataSubscription.unsubscribe();
        }

        if (this._submitDynamicFormSubscription) {
            this._submitDynamicFormSubscription.unsubscribe();
        }

        if (this.dynamicForm) {
            this.dynamicForm.dispose();
        }
    }
}
