import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';
import { AppPaths } from 'app/app.paths';

import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';
import { setPasswordRequestModelName } from 'api/request-models/set-password.request-model';


@Component({
    templateUrl: './change-password.component.html'
})
export class ChangePasswordComponent {
    static requestModelName = setPasswordRequestModelName;
    static formSubmitRelativeUrl = AccountControllerRelativeUrls.setPassword;

    manageAccountPath: string = AppPaths.manageAccountPath;

    constructor(private _router: Router) {
    }

    onSubmitSuccess(event: SubmitEventModel): void {
        this._router.navigate([this.manageAccountPath]);
    }
}
