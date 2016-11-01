import { Component, Input, Output, OnInit, OnDestroy, EventEmitter } from '@angular/core';
import { Router } from '@angular/router/index';
import { Response } from '@angular/http';
import { Subscription } from 'rxjs';

import { DynamicForm } from './dynamic-form';
import { DynamicFormsService } from '../dynamic-forms.service';
import { ErrorHandlerService } from '../../utility/error-handler.service';

@Component({
    selector: 'dynamic-form',
    templateUrl: 'dynamic-form.component.html'
})
export class DynamicFormComponent implements OnInit, OnDestroy {
    @Input() formModelName: string;
    @Input() formSubmitUrl: string;
    @Output() submitSuccess = new EventEmitter<Response>();
    @Output() submitError = new EventEmitter<Response | any>();

    // create resolve guard to this does not need to be initialized
    dynamicForm: DynamicForm = new DynamicForm([], null);

    private _getDynamicFormSubscription: Subscription;
    private _submitDynamicFormSubscription: Subscription;

    constructor(private _dynamicFormsService: DynamicFormsService, private _router: Router, private _errorHandlerService: ErrorHandlerService) { }

    /**
     * Retrieves and sets dynamicForm
     */
    ngOnInit(): void {
        this._getDynamicFormSubscription = this._dynamicFormsService
            .getDynamicForm(this.formModelName)
            .subscribe(
            dynamicForm => {
                this.dynamicForm = dynamicForm;
            },
            error => this._errorHandlerService.handleUnexpectedError(this._router, error));
    }

    /**
     * Sends json representation of dynamicForm values if form is valid
     */
    onSubmit(event: Event): void {
        event.preventDefault();

        if (this.dynamicForm.onSubmit()) {
            this._submitDynamicFormSubscription = this.
                _dynamicFormsService.
                submitDynamicForm(this.formSubmitUrl, this.dynamicForm).
                subscribe(
                    (response: Response) => this.submitSuccess.emit(response),
                    (error: Response | any) => this.submitError.emit(error)
                );
        }
    }

    ngOnDestroy(): void {
        this.submitError.unsubscribe();
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
