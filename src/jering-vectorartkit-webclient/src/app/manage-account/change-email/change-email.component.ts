import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { SetEmailResponseModel } from '../../shared/response-models/set-email.response-model';

@Component({
    templateUrl: './change-email.component.html'
})
export class ChangeEmailComponent {
    static formModelName = `SetEmail`;
    static formSubmitRelativeUrl = `Account/SetEmail`;

    constructor(private _router: Router) {
    }

    onSubmitSuccess(responseModel: SetEmailResponseModel): void {
        this._router.navigate([`/manage-account`]);
    }
}
