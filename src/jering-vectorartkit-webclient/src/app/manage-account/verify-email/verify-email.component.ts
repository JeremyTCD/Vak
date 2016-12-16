import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router/index';

import { SetEmailVerifiedResponseModel } from '../../shared/response-models/set-email-verified.response-model';
import { DynamicFormComponent } from '../../shared/dynamic-forms/dynamic-form/dynamic-form.component';
import { UserService } from '../../shared/user.service';

@Component({
    templateUrl: './verify-email.component.html'
})
export class VerifyEmailComponent implements OnInit {
    linkExpiredOrInvalid: boolean = false;

    constructor(private _activatedRoute: ActivatedRoute, public userService: UserService) {
    }

    ngOnInit(): void {
        let responseModel: SetEmailVerifiedResponseModel = this._activatedRoute.snapshot.data[`responseModel`];

        if (responseModel && responseModel.invalidAccountId || responseModel.invalidToken) {
            this.linkExpiredOrInvalid = true;
        }
    }
}
