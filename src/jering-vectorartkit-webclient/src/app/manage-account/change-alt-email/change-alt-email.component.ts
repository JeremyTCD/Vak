import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { SetAltEmailResponseModel } from '../../shared/response-models/set-alt-email.response-model';

@Component({
    templateUrl: './change-alt-email.component.html'
})
export class ChangeAltEmailComponent {
    static formModelName = `SetAltEmail`;
    static formSubmitRelativeUrl = `Account/SetAltEmail`;

    constructor(private _router: Router) {
    }

    onSubmitSuccess(responseModel: SetAltEmailResponseModel): void {
        this._router.navigate([`/manage-account`]);
    }
}
