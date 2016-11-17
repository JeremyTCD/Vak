﻿import { Injectable } from '@angular/core';
import { Response } from '@angular/http';
import { Router } from '@angular/router';
import { Check } from './check';

import { environment } from '../../../environments/environment';
import { ErrorResponseModel } from '../response-models/error.response-model';

/**
 * Provides error handling
 */
@Injectable()
export class ErrorHandlerService {
    constructor(private _router: Router) { }

    handleUnexpectedError(error: any): void {
        if (!Check.isValue(error)){
            this._router.navigate([`/error`]);
        }

        if (Check.isString(error)) {
            this._router.navigate([`/error`, { errorMessage: error }]);
            return;
        }

        let errorMessage: string;

        // TODO should inserted string be escaped etc? read up on xss and angular2
        if (environment.production) {
            errorMessage = (error as ErrorResponseModel).errorMessage
        } else {
            // Print entire object
            for (let key in error) {
                errorMessage += `${key}: ${error[key]} -- `;
            }
        }

        if (Check.isValue(errorMessage)) {
            this._router.navigate([`/error`,
                {
                    errorMessage: errorMessage
                }
            ]);

            return;
        }

        this._router.navigate([`/error`]);
    }
}