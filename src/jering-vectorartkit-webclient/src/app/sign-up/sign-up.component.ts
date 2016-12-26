import { Component } from '@angular/core';
import { Router } from '@angular/router/index';
import { Response } from '@angular/http';

import { environment } from 'environments/environment';

import { UserService } from 'app/shared/user.service';
import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';
import { AppPaths } from 'app/app.paths';

import { SignUpResponseModel} from 'api/response-models/sign-up.response-model';
import { SignUpRequestModel } from 'api/request-models/sign-up.request-model';
import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';

@Component({
    templateUrl: './sign-up.component.html'
})
export class SignUpComponent {
    static requestModelName = `SignUp`;
    static formSubmitRelativeUrl = AccountControllerRelativeUrls.signUp;

    // TODO: Fetch this for localization
    termsAndConditions: string = `Agree with terms and conditions`;
    logInPath: string = AppPaths.logInPath;

    constructor(private _router: Router, private _userService: UserService) {
    }

    onSubmitSuccess(event: SubmitEventModel): void {
        let requestModel = event.requestModel as SignUpRequestModel;

        this._router.navigate([AppPaths.homePath]);
        // Store username in persistent storage to minimize engagement barriers
        this._userService.logIn(requestModel.email, true);
    }
}
