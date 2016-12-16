import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { SetDisplayNameResponseModel } from '../../shared/response-models/set-display-name.response-model';

@Component({
    templateUrl: './change-display-name.component.html'
})
export class ChangeDisplayNameComponent {
    static formModelName = `SetDisplayName`;
    static formSubmitRelativeUrl = `Account/SetDisplayName`;

    constructor(private _router: Router) {
    }

    onSubmitSuccess(responseModel: SetDisplayNameResponseModel): void {
        this._router.navigate([`/manage-account`]);
    }
}
