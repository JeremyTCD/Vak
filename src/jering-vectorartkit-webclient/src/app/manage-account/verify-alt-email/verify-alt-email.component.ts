import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router/index';

import { SetAltEmailVerifiedResponseModel } from 'api/response-models/set-alt-email-verified.response-model';
import { DynamicFormComponent } from 'app/shared/dynamic-forms/dynamic-form/dynamic-form.component';

import { AppPaths } from 'app/app.paths';

@Component({
    templateUrl: './verify-alt-email.component.html'
})
export class VerifyAltEmailComponent implements OnInit {
    linkExpiredOrInvalid: boolean = false;

    manageAccountPath: string = AppPaths.manageAccountPath;

    constructor(private _activatedRoute: ActivatedRoute) {
    }

    ngOnInit(): void {
        let responseModel: SetAltEmailVerifiedResponseModel = this._activatedRoute.snapshot.data[`responseModel`];

        // Api only returns a response model for errors. Unexpected errors are filtered out by HttpService.
        if (responseModel) {
            this.linkExpiredOrInvalid = true;
        }
    }
}
