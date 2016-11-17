import { Injectable } from '@angular/core';
import { Response } from '@angular/http';
import { Router } from '@angular/router';
import { Check } from './check';

import { environment } from '../../../environments/environment';

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
            if (error instanceof Response) {
                // Web Api should send an error message with key `errorMessage`
                 errorMessage = error.json().errorMessage;
            }
        } else {
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