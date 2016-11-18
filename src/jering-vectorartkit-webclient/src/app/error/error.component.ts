import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Subscription } from 'rxjs';
import 'rxjs/add/operator/catch';

import { Check } from '../shared/check';

@Component({
    templateUrl: './error.component.html'
})
export class ErrorComponent implements OnInit, OnDestroy {
    public errorMessage: string;
    private _paramsSubscription: Subscription;

    constructor(private _activatedRoute: ActivatedRoute) {
    }

    ngOnInit(): void {
        this._paramsSubscription = this._activatedRoute.
            params.
            subscribe(
                (params: Params) => {
                    let errorMessage = params['errorMessage'];
                    this.errorMessage = Check.isValue(errorMessage) ? errorMessage : null;
                },
                error => {
                    this.errorMessage = null;
                }
            );
    }

    ngOnDestroy(): void {
        if (this._paramsSubscription) {
            this._paramsSubscription.unsubscribe();
        }
    }
}
