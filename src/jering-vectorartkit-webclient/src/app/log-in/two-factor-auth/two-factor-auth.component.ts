import { Component, AfterViewInit, ViewChild, OnDestroy, OnInit } from '@angular/core';
import { Router, ActivatedRoute, ActivatedRouteSnapshot } from '@angular/router/index';
import { Response } from '@angular/http';
import { Subscription } from 'rxjs';

import { UserService } from 'app/shared/user.service';
import { DynamicFormComponent } from 'app/shared/dynamic-forms/dynamic-form/dynamic-form.component';
import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';
import { AppPaths } from 'app/app.paths';

import { TwoFactorLogInResponseModel } from 'api/response-models/two-factor-log-in.response-model';
import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';

@Component({
    templateUrl: './two-factor-auth.component.html'
})
export class TwoFactorAuthComponent implements AfterViewInit {
    static requestModelName = `TwoFactorLogIn`;
    static formSubmitRelativeUrl = AccountControllerRelativeUrls.twoFactorLogIn;

    @ViewChild(`dynamicFormComponent`) dynamicFormComponent: DynamicFormComponent;

    codeExpired: boolean = false;
    isPersistent: boolean;
    username: string;
    returnUrl: string;
    logInPath: string = AppPaths.logInPath;

    constructor(private _router: Router, private _userService: UserService,
        private _activatedRoute: ActivatedRoute) {
    }

    ngOnInit(): void {
        this.isPersistent = this._activatedRoute.snapshot.params[`isPersistent`] == `true`;
        this.username = this._activatedRoute.snapshot.params[`username`];
        this.returnUrl = this._activatedRoute.snapshot.params[`returnUrl`];
    }

    ngAfterViewInit(): void {
        this.dynamicFormComponent.dynamicForm.getDynamicControl(`isPersistent`).value = this.isPersistent.toString();
    }

    onSubmitSuccess(event: SubmitEventModel): void {
        this._userService.logIn(this.username, this.isPersistent);
        this._router.navigate([this.returnUrl ? this.returnUrl : AppPaths.homePath]);
    }

    onSubmitError(event: SubmitEventModel): void {
        let responseModel = event.responseModel as TwoFactorLogInResponseModel;

        if (responseModel.expiredCredentials) {
            // Prompt user to re log in
            this.codeExpired = true;
        } 
    }
}
