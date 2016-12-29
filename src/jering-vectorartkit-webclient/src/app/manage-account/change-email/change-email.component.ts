import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';
import { UserService } from 'app/shared/user.service';
import { AppPaths } from 'app/app.paths';

import { SetEmailRequestModel, setEmailRequestModelName } from 'api/request-models/set-email.request-model';
import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';

@Component({
    templateUrl: './change-email.component.html'
})
export class ChangeEmailComponent {
    static requestModelName = setEmailRequestModelName;
    static formSubmitRelativeUrl = AccountControllerRelativeUrls.setEmail;

    manageAccountPath: string = AppPaths.manageAccountPath;

    constructor(private _router: Router , private _userService: UserService) {
    }

    onSubmitSuccess(event: SubmitEventModel): void {
        let requestModel = event.requestModel as SetEmailRequestModel;

        this._userService.changeUsername(requestModel.newEmail);
        this._router.navigate([this.manageAccountPath]);
    }
}
