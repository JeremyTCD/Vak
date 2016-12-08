import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { ChangePasswordResponseModel } from '../../shared/response-models/change-password.response-model';

@Component({
    templateUrl: './change-password.component.html'
})
export class ChangePasswordComponent {
    static formModelName = `ChangePassword`;
    static formSubmitRelativeUrl = `Account/ChangePassword`;

    constructor(private _router: Router) {
    }

    onSubmitSuccess(responseModel: ChangePasswordResponseModel): void {
        this._router.navigate([`/manage-account`]);
    }
}
