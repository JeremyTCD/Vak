import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';
import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';
import { AppPaths } from 'app/app.paths';

@Component({
    templateUrl: './two-factor-verify-email.component.html'
})
export class TwoFactorVerifyEmailComponent {
    static requestModelName = `TwoFactorVerifyEmail`;
    static formSubmitRelativeUrl = AccountControllerRelativeUrls.twoFactorVerifyEmail;

    manageAccountPath: string = AppPaths.manageAccountPath;

    constructor(private _router: Router) {
    }

    onSubmitSuccess(event: SubmitEventModel): void {
        this._router.navigate([this.manageAccountPath]);
    }
}
