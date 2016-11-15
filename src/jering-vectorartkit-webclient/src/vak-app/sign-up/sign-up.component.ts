import { Component } from '@angular/core';
import { Router } from '@angular/router/index';
import { Response } from '@angular/http';

import { environment } from '../../environments/environment';
import { UserService } from '../shared/user.service';
import { SignUpSubmitSuccessData } from './sign-up-submit-success-data';

@Component({
    templateUrl: './sign-up.component.html'
})
export class SignUpComponent {
    static formModelName = `SignUp`;
    static formSubmitRelativeUrl = `Account/SignUp`;

    constructor(private _router: Router, private _userService: UserService) {
    }

    onSubmitSuccess(data: SignUpSubmitSuccessData): void {
        this._router.navigate([`/home`]);
        this._userService.logIn(data.username, false);
    }
}
