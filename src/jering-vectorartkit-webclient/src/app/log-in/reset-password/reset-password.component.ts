import { Component, AfterViewInit, ViewChild, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router/index';

import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';
import { DynamicFormComponent } from 'app/shared/dynamic-forms/dynamic-form/dynamic-form.component';
import { AppPaths } from 'app/app.paths';

import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';
import { ResetPasswordResponseModel } from 'api/response-models/reset-password.response-model';
import { resetPasswordRequestModelName } from 'api/request-models/reset-password.request-model';


@Component({
    templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent implements AfterViewInit, OnInit {
    static requestModelName = resetPasswordRequestModelName;
    static formSubmitRelativeUrl = AccountControllerRelativeUrls.resetPassword;

    @ViewChild(`dynamicFormComponent`) dynamicFormComponent: DynamicFormComponent;

    email: string;
    token: string;
    linkExpiredOrInvalid: boolean;
    passwordResetSuccessful: boolean;

    logInPath: string = AppPaths.logInPath;
    forgotPasswordPath: string = AppPaths.forgotPasswordPath;

    constructor(private _activatedRoute: ActivatedRoute) {
    }

    ngOnInit(): void {
        this.email = this._activatedRoute.snapshot.params[`email`];
        this.token = this._activatedRoute.snapshot.params[`token`];
    }

    ngAfterViewInit(): void {
        this.dynamicFormComponent.dynamicForm.getDynamicControl(`email`).value = this.email;
        this.dynamicFormComponent.dynamicForm.getDynamicControl(`token`).value = this.token;
    }

    onSubmitSuccess(event: SubmitEventModel): void {
        this.passwordResetSuccessful = true;
    }

    onSubmitError(event: SubmitEventModel): void {
        let responseModel = event.responseModel as ResetPasswordResponseModel;

        if (responseModel.invalidEmail || responseModel.invalidToken) {
            this.linkExpiredOrInvalid = true;
        }
    }
}
