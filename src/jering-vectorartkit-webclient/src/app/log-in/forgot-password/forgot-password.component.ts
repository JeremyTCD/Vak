import { Component } from '@angular/core';

@Component({
    templateUrl: './forgot-password.component.html'
})
export class ForgotPasswordComponent {
    static formModelName = `SendResetPasswordEmail`;
    static formSubmitRelativeUrl = `Account/SendResetPasswordEmail`;

    submitSuccessful: boolean;

    constructor() {
    }

    onSubmitSuccess(responseModel: any): void {
        this.submitSuccessful = true;
    }
}
