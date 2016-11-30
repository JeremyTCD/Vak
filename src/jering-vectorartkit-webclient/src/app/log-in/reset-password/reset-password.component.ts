import { Component, AfterViewInit, ViewChild, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router/index';

import { ResetPasswordResponseModel } from '../../shared/response-models/reset-password.response-model';
import { DynamicFormComponent } from '../../shared/dynamic-forms/dynamic-form/dynamic-form.component';

@Component({
    templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent implements AfterViewInit, OnInit {
    static formModelName = `ResetPassword`;
    static formSubmitRelativeUrl = `Account/ResetPassword`;

    @ViewChild(`dynamicFormComponent`) dynamicFormComponent: DynamicFormComponent;

    email: string;
    token: string;
    linkExpiredOrInvalid: boolean;
    passwordResetSuccessful: boolean;

    constructor(private _activatedRoute: ActivatedRoute) {
    }

    ngOnInit(): void {
        this.email = this._activatedRoute.snapshot.params[`email`];
        this.token = this._activatedRoute.snapshot.params[`token`];
    }

    ngAfterViewInit(): void {
        this.dynamicFormComponent.dynamicForm.getDynamicControl(`Email`).value = this.email;
        this.dynamicFormComponent.dynamicForm.getDynamicControl(`Token`).value = this.token;
    }

    onSubmitSuccess(responseModel: ResetPasswordResponseModel): void {
        this.passwordResetSuccessful = true;
    }

    onSubmitError(responseModel: ResetPasswordResponseModel): void {
        if (responseModel.linkExpiredOrInvalid) {
            this.linkExpiredOrInvalid = true;
        }
    }
}
