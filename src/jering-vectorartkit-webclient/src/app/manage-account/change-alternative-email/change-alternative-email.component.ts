import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { ChangeAlternativeEmailResponseModel } from '../../shared/response-models/change-alternative-email.response-model';

@Component({
    templateUrl: './change-alternative-email.component.html'
})
export class ChangeAlternativeEmailComponent {
    static formModelName = `ChangeAlternativeEmail`;
    static formSubmitRelativeUrl = `Account/ChangeAlternativeEmail`;

    constructor(private _router: Router) {
    }

    onSubmitSuccess(responseModel: ChangeAlternativeEmailResponseModel): void {
        this._router.navigate([`/manage-account`]);
    }
}
