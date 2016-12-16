import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { SetPasswordResponseModel } from '../../shared/response-models/set-password.response-model';

@Component({
    templateUrl: './change-password.component.html'
})
export class ChangePasswordComponent {
    static formModelName = `SetPassword`;
    static formSubmitRelativeUrl = `Account/SetPassword`;

    constructor(private _router: Router) {
    }

    onSubmitSuccess(responseModel: SetPasswordResponseModel): void {
        this._router.navigate([`/manage-account`]);
    }
}
