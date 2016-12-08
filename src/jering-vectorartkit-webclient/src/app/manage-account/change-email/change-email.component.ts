import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { ChangeEmailResponseModel } from '../../shared/response-models/change-email.response-model';

@Component({
    templateUrl: './change-email.component.html'
})
export class ChangeEmailComponent {
    static formModelName = `ChangeEmail`;
    static formSubmitRelativeUrl = `Account/ChangeEmail`;

    constructor(private _router: Router) {
    }

    onSubmitSuccess(responseModel: ChangeEmailResponseModel): void {
        this._router.navigate([`/manage-account`]);
    }
}
