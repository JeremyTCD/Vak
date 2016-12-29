import { Component } from '@angular/core';

import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';

import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';
import { sendResetPasswordEmailRequestModelName } from 'api/request-models/send-reset-password-email.request-model';

import { AppPaths } from 'app/app.paths';

@Component({
    templateUrl: './forgot-password.component.html'
})
export class ForgotPasswordComponent {
    static requestModelName = sendResetPasswordEmailRequestModelName;
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
