import { Component } from '@angular/core';

import { SendResetPasswordEmailResponseModel } from '../../shared/response-models/send-reset-password-email.response-model';

@Component({
    templateUrl: './forgot-password.component.html'
})
export class ForgotPasswordComponent {
    static formModelName = `SendResetPasswordEmail`;
    static formSubmitRelativeUrl = `Account/SendResetPasswordEmail`;

    submitSuccessful: boolean;

    constructor() {
    }

    onSubmitSuccess(responseModel: SendResetPasswordEmailResponseModel): void {
        this.submitSuccessful = true;
    }

    onSubmitError(responseModel: SendResetPasswordEmailResponseModel): void {
        // Avoid indicating to client whether email is valid
        this.submitSuccessful = true;
    }
}
