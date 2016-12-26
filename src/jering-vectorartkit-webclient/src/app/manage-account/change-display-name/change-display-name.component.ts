import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';
import { AppPaths } from 'app/app.paths';

import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';

@Component({
    templateUrl: './change-display-name.component.html'
})
export class ChangeDisplayNameComponent {
    static requestModelName = `SetDisplayName`;
    static formSubmitRelativeUrl = AccountControllerRelativeUrls.setDisplayName;

    manageAccountPath: string = AppPaths.manageAccountPath;

    constructor(private _router: Router) {
    }

    onSubmitSuccess(event: SubmitEventModel): void {
        this._router.navigate([this.manageAccountPath]);
    }
}
