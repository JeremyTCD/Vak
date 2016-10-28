import { Component } from '@angular/core';
import { Router } from '@angular/router/index';
import { Response } from '@angular/http';

import { environment } from '../../environments/environment';
import { handleUnexpectedError } from '../shared/utility/error-handling';

@Component({
    templateUrl: './sign-up.component.html'
})
export class SignUpComponent {
    formModelName = `SignUp`;
    formSubmitUrl = `${environment.apiUrl}Account/SignUp`;

    constructor(private _router: Router) {
    }

    onSubmitSuccess(response: Response): void {
        this._router.navigate([`/home`]);
    }

    onSubmitError(error: Response | any): void {
        handleUnexpectedError(this._router, error);
    }
}
