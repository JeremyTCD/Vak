import { Component } from '@angular/core';

import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';

import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';

import { AppPaths } from 'app/app.paths';

@Component({
    templateUrl: './forgot-password.component.html'
})
export class ForgotPasswordComponent {
    static requestModelName = `SendResetPasswordEmail`;
    static formSubmitRelativeUrl = AccountControllerRelativeUrls.sendResetPasswordEmail;

    submitSuccessful: boolean;
    logInPath: string = AppPaths.logInPath;

    onSubmitSuccess(event: SubmitEventModel): void {
        this.submitSuccessful = true;
    }

    onSubmitError(event: SubmitEventModel): void {
        // Avoid indicating to client whether email is valid
        this.submitSuccessful = true;
    }
}
