import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router, ActivatedRoute, ActivatedRouteSnapshot } from '@angular/router/index';
import { Response } from '@angular/http';

import { UserService } from 'app/shared/user.service';
import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';
import { AppPaths } from 'app/app.paths';

import { LogInResponseModel } from 'api/response-models/log-in.response-model';
import { LogInRequestModel, logInRequestModelName } from 'api/request-models/log-in.request-model';
import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';

@Component({
    templateUrl: './log-in.component.html'
})
export class LogInComponent {
    static requestModelName = logInRequestModelName;
    static formSubmitRelativeUrl = AccountControllerRelativeUrls.logIn;

    forgotPasswordPath: string = AppPaths.forgotPasswordPath;
    signUpPath: string = AppPaths.signUpPath;
    returnUrl: string;

    constructor(private _router: Router, private _userService: UserService,
        private _activatedRoute: ActivatedRoute) {
    }

    ngOnInit(): void {
        this.returnUrl = this._activatedRoute.snapshot.params[`returnUrl`];
    }

    onSubmitSuccess(event: SubmitEventModel): void {
        let requestModel: LogInRequestModel = event.requestModel as LogInRequestModel;

        this._userService.logIn(requestModel.email, requestModel.rememberMe === `true`);
        this._router.navigate([this.returnUrl ? this.returnUrl : AppPaths.homePath]);
    }

    onSubmitError(event: SubmitEventModel): void {
        let responseModel = event.responseModel as LogInResponseModel;
        let requestModel = event.requestModel as LogInRequestModel;

        if (responseModel.twoFactorRequired) {
            this._router.navigate([AppPaths.twoFactorAuthPath, {
                username: requestModel.email,
                isPersistent: requestModel.rememberMe,
                returnUrl: this.returnUrl
            }]);
        } 
    }
}
