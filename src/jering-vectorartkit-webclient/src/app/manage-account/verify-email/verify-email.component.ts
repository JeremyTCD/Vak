import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router/index';

import { SetEmailVerifiedResponseModel } from 'api/response-models/set-email-verified.response-model';
import { DynamicFormComponent } from 'app/shared/dynamic-forms/dynamic-form/dynamic-form.component';

import { AppPaths } from 'app/app.paths';

@Component({
    templateUrl: './verify-email.component.html'
})
export class VerifyEmailComponent implements OnInit {
    linkExpiredOrInvalid: boolean = false;

    manageAccountPath: string = AppPaths.manageAccountPath;

    constructor(private _activatedRoute: ActivatedRoute) {
    }

    ngOnInit(): void {
        let responseModel: SetEmailVerifiedResponseModel = this._activatedRoute.snapshot.data[`responseModel`];

        // Api only returns a response model for errors. Unexpected errors are filtered out by HttpService
        if (responseModel) {
            this.linkExpiredOrInvalid = true;
        }
    }
}
