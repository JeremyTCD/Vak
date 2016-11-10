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

    onSubmitSuccess(): void {
        this._router.navigate([`/home`]);
    }
}
