import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router/index';
import { Subscription } from 'rxjs';

import { GetAccountDetailsResponseModel} from '../shared/response-models/get-account-details.response-model';
import { HttpService } from '../shared/http.service';
import { SetTwoFactorEnabledResponseModel } from '../shared/response-models/set-two-factor-enabled.response-model';

@Component({
    templateUrl: './manage-account.component.html'
})
export class ManageAccountComponent implements OnInit, OnDestroy {
    private _dataSubscription: Subscription;

    accountDetails: GetAccountDetailsResponseModel;

    constructor(private _activatedRoute: ActivatedRoute, private _httpService: HttpService, private _router: Router) {}

    emailVerificationEmailSent: boolean = false;
    altEmailVerificationEmailSent: boolean = false;

    /**
     * Retrieves and sets responseModel
     */
    ngOnInit(): void {
        this._dataSubscription = this.
            _activatedRoute.
            data.
            subscribe((data: { responseModel: GetAccountDetailsResponseModel}) => {
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
            post(`Account/SendEmailVerificationEmail`, null).
            subscribe(
                responseModel => this.emailVerificationEmailSent = true                                  
            );
    }

    sendAltEmailVerificationEmail(): void {
        this.
            _httpService.
            post(`Account/SendAltEmailVerificationEmail`, null).
            subscribe(
                responseModel => this.altEmailVerificationEmailSent = true,
                responseModel => { } // Do nothing. SendAltEmailVerificationEmail sends expected error if alt email is not set
            );
    }

    setTwoFactorEnabled(enabled: boolean): void {
        this.
            _httpService.
            post(`Account/SetTwoFactorEnabled`, { enabled: enabled }).
            subscribe(
                responseModel => this.accountDetails.twoFactorEnabled = enabled,
                responseModel => this._router.navigate([`two-factor-verify-email`]) // email not verified is the only expected error SetTwoFactorEnabled returns                                     
            );
    }
}
