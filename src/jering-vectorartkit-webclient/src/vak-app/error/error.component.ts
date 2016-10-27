import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import 'rxjs/add/operator/catch';

import { Check } from '../shared/utility/check';

@Component({
    templateUrl: './error.component.html'
})
export class ErrorComponent implements OnInit {
    public errorMessage: string;

    constructor(private _activatedRoute: ActivatedRoute) {
    }

    ngOnInit(): void {
        this._activatedRoute.
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
}
