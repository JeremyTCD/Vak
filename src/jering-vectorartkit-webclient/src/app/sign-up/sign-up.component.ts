import { Component } from '@angular/core';
import { Router } from '@angular/router/index';
import { Response } from '@angular/http';

import { environment } from '../../environments/environment';
import { UserService } from '../shared/user.service';
import { SignUpResponseModel} from '../shared/response-models/sign-up.response-model';

@Component({
    templateUrl: './sign-up.component.html'
})
export class SignUpComponent {
    static formModelName = `SignUp`;
    static formSubmitRelativeUrl = `Account/SignUp`;

    constructor(private _router: Router, private _userService: UserService) {
    }

    onSubmitSuccess(responseModel: SignUpResponseModel): void {
        this._router.navigate([`/home`]);
        // Store username in persistent storage to minimize engagement barriers
        this._userService.logIn(responseModel.username, true);
    }
}
