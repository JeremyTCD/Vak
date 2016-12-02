import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router/index';
import { Subscription } from 'rxjs';

import { GetAccountDetailsResponseModel} from '../shared/response-models/get-account-details.response-model';

@Component({
    templateUrl: './manage-account.component.html'
})
export class ManageAccountComponent implements OnInit, OnDestroy {
    private _dataSubscription: Subscription;

    responseModel: GetAccountDetailsResponseModel;

    constructor(private _activatedRoute: ActivatedRoute) {}

    /**
     * Retrieves and sets responseModel
     */
    ngOnInit(): void {
        this._dataSubscription = this.
            _activatedRoute.
            data.
            subscribe((data: { responseModel: GetAccountDetailsResponseModel}) => {
                this.responseModel = data.responseModel;
            });
    }

    ngOnDestroy(): void {
        if (this._dataSubscription) {
            this._dataSubscription.unsubscribe();
        }
    }
}
