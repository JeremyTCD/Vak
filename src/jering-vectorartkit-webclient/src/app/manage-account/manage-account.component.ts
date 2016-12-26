import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router/index';
import { Subscription } from 'rxjs';

import { HttpService } from 'app/shared/http.service';
import { AppPaths } from 'app/app.paths';

import { GetAccountDetailsResponseModel } from 'api/response-models/get-account-details.response-model';
import { SetTwoFactorEnabledResponseModel } from 'api/response-models/set-two-factor-enabled.response-model';
import { SetTwoFactorEnabledRequestModel } from 'api/request-models/set-two-factor-enabled.request-model';
import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';

@Component({
    templateUrl: './manage-account.component.html'
})
export class ManageAccountComponent implements OnInit, OnDestroy {
    private _dataSubscription: Subscription;

    accountDetails: GetAccountDetailsResponseModel;

    constructor(private _activatedRoute: ActivatedRoute, private _httpService: HttpService, private _router: Router) { }

    emailVerificationEmailSent: boolean = false;
    altEmailVerificationEmailSent: boolean = false;
    changeEmailPath: string = AppPaths.changeEmailPath;
    changeAltEmailPath: string = AppPaths.changeAltEmailPath;
    changeDisplayNamePath: string = AppPaths.changeDisplayNamePath;
    changePasswordPath: string = AppPaths.changePasswordPath;

    /**
     * Retrieves and sets responseModel
     */
    ngOnInit(): void {
        this._dataSubscription = this.
            _activatedRoute.
            data.
            subscribe((data: { responseModel: GetAccountDetailsResponseModel }) => {
                this.accountDetails = data.responseModel;
            });
    }

    ngOnDestroy(): void {
        if (this._dataSubscription) {
            this._dataSubscription.unsubscribe();
        }
    }

    sendEmailVerificationEmail(): void {
        this.
            _httpService.
            post(AccountControllerRelativeUrls.sendEmailVerificationEmail, null).
            subscribe(
            responseModel => this.emailVerificationEmailSent = true
            );
    }

    sendAltEmailVerificationEmail(): void {
        this.
            _httpService.
            post(AccountControllerRelativeUrls.sendAltEmailVerificationEmail, null).
            subscribe(
            responseModel => this.altEmailVerificationEmailSent = true
            );
    }

    setTwoFactorEnabled(enabled: boolean): void {
        let requestModel: SetTwoFactorEnabledRequestModel = { enabled: enabled.toString() };

        this.
            _httpService.
            post(AccountControllerRelativeUrls.setTwoFactorEnabled, requestModel).
            subscribe(
            responseModel => this.accountDetails.twoFactorEnabled = enabled,
            responseModel => this._router.navigate([AppPaths.twoFactorVerifyEmailPath]) // email not verified is the only expected error SetTwoFactorEnabled returns                                     
            );
    }
}
