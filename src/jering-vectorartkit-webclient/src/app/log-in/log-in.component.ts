import { Component, OnDestroy } from '@angular/core';
import { Router } from '@angular/router/index';
import { Response } from '@angular/http';

import { UserService } from '../shared/user.service';
import { LogInResponseModel} from '../shared/response-models/log-in.response-model';

@Component({
    templateUrl: './log-in.component.html'
})
export class LogInComponent {
    static formModelName = `LogIn`;
    static formSubmitRelativeUrl = `Account/LogIn`;

    constructor(private _router: Router, private _userService: UserService) {
    }

    onSubmitSuccess(responseModel: LogInResponseModel): void {
        if (responseModel.twoFactorRequired) {
            this._router.navigate([`/login/twofactor`]);
        }
        this._userService.logIn(responseModel.username, responseModel.isPersistent);
        // navigate to return url or to home
        let returnUrl = this._userService.returnUrl;
        this._userService.returnUrl = null;
        this._router.navigate([returnUrl ? returnUrl : `/home`]);
    }
}
