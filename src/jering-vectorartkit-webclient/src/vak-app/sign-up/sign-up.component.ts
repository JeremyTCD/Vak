import { Component } from '@angular/core';
import { Router } from '@angular/router/index';
import { Response } from '@angular/http';

import { environment } from '../../environments/environment';
import { ErrorHandlerService } from '../shared/utility/error-handler.service';

@Component({
    templateUrl: './sign-up.component.html'
})
export class SignUpComponent {
    formModelName = `SignUp`;
    formSubmitUrl = `${environment.apiUrl}Account/SignUp`;

    constructor(private _router: Router, private _errorHandlerService: ErrorHandlerService) {
    }

    onSubmitSuccess(response: Response): void {
        this._router.navigate([`/home`]);
    }

    onSubmitError(error: Response | any): void {
        this._errorHandlerService.handleUnexpectedError(this._router, error);
    }
}
