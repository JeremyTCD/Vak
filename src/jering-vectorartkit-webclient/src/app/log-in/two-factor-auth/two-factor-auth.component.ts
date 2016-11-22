import { Component, AfterViewInit, ViewChild, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute, ActivatedRouteSnapshot } from '@angular/router/index';
import { Response } from '@angular/http';
import { Subscription } from 'rxjs';

import { UserService } from '../../shared/user.service';
import { TwoFactorLogInResponseModel } from '../../shared/response-models/two-factor-log-in.response-model';
import { DynamicFormComponent } from '../../shared/dynamic-forms/dynamic-form/dynamic-form.component';

@Component({
    templateUrl: './two-factor-auth.component.html'
})
export class TwoFactorAuthComponent implements AfterViewInit {
    static formModelName = `TwoFactorLogIn`;
    static formSubmitRelativeUrl = `Account/TwoFactorLogIn`;

    @ViewChild(`dynamicFormComponent`) dynamicFormComponent: DynamicFormComponent;

    constructor(private _router: Router, private _userService: UserService,
        private _activatedRoute: ActivatedRoute) {
    }

    ngAfterViewInit(): void {
        this.dynamicFormComponent.dynamicForm.getDynamicControl(`IsPersistent`).value = this._activatedRoute.snapshot.params[`isPersistent`];
    }

    onSubmitSuccess(responseModel: TwoFactorLogInResponseModel): void {
        this._userService.logIn(responseModel.username, responseModel.isPersistent);
        // navigate to return url or to home
        let returnUrl = this._activatedRoute.snapshot.params[`returnUrl`];
        this._router.navigate([returnUrl ? returnUrl : `/home`]);
    }
}
