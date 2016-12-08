import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { ChangeDisplayNameResponseModel } from '../../shared/response-models/change-display-name.response-model';

@Component({
    templateUrl: './change-display-name.component.html'
})
export class ChangeDisplayNameComponent {
    static formModelName = `ChangeDisplayName`;
    static formSubmitRelativeUrl = `Account/ChangeDisplayName`;

    constructor(private _router: Router) {
    }

    onSubmitSuccess(responseModel: ChangeDisplayNameResponseModel): void {
        this._router.navigate([`/manage-account`]);
    }
}
