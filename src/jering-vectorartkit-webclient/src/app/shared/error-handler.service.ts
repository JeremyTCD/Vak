import { Injectable } from '@angular/core';
import { Response } from '@angular/http';
import { Router } from '@angular/router';
import { Location } from '@angular/common';

import { environment } from 'environments/environment';

import { ErrorResponseModel } from 'api/response-models/error.response-model';

import { AppPaths } from 'app/app.paths';

import { Check } from './check';
import { UserService } from './user.service';

/**
 * Provides error handling
 */
@Injectable()
export class ErrorHandlerService {
    constructor(private _router: Router, private _location: Location, private _userService: UserService) { }

    /**
     * Handle unauthorized errors by navigating to log in component
     */
    handleUnauthorizedError(): void {
        this._userService.logOff();
        this._router.navigate([AppPaths.logInPath, { returnUrl: this._location.path() }]);
    }

    /**
     * Handle unexpected errors that cannot be recovered from
     */
    handleUnexpectedError(error: any): void {
        if (!Check.isValue(error)){
            this._router.navigate([AppPaths.errorPath]);
        }

        if (Check.isString(error)) {
            this._router.navigate([AppPaths.errorPath, { errorMessage: error }]);
            return;
        }

        let errorMessage: string;

        // TODO should inserted string be escaped etc? read up on xss and angular2
        if (environment.production) {
            errorMessage = (error as ErrorResponseModel).errorMessage;
        } else {
            // Print entire object
            for (let key in error) {
                errorMessage += `${key}: ${error[key]} -- `;
            }
        }

        if (Check.isValue(errorMessage)) {
            this._router.navigate([AppPaths.errorPath,
                {
                    errorMessage: errorMessage
                }
            ]);

            return;
        }

        this._router.navigate([AppPaths.errorPath]);
    }
}