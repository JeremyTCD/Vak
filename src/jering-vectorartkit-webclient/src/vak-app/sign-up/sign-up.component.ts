import { Component } from '@angular/core';
import { Router } from '@angular/router/index';
import { Response } from '@angular/http';

import { environment } from '../../environments/environment';

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
        if (error instanceof Response) {
            this._router.navigate([`/error`, { errorMessage: error.json().errorMessage }]);
        }
        else {
            this._router.navigate([`/error`]);
        }
    }
}
