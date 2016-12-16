import { Component, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute, ActivatedRouteSnapshot } from '@angular/router/index';
import { Response } from '@angular/http';

import { UserService } from '../shared/user.service';
import { LogInResponseModel} from '../shared/response-models/log-in.response-model';

@Component({
    templateUrl: './log-in.component.html'
})
export class LogInComponent {
    static formModelName = `LogIn`;
    static formSubmitRelativeUrl = `Account/LogIn`;

    constructor(private _router: Router, private _userService: UserService,
        private _activatedRoute: ActivatedRoute) {
    }

    onSubmitSuccess(responseModel: LogInResponseModel): void {
        let returnUrl = this._activatedRoute.snapshot.params[`returnUrl`];

        this._userService.logIn(responseModel.username, responseModel.isPersistent);
        this._router.navigate([returnUrl ? returnUrl : `/home`]);
    }

    onSubmitError(responseModel: LogInResponseModel): void {
        if (responseModel.twoFactorRequired) {
            let returnUrl = this._activatedRoute.snapshot.params[`returnUrl`];

            this._router.navigate([`/login/two-factor-auth`, { isPersistent: responseModel.isPersistent, returnUrl: returnUrl }]);
        } 
    }
}
