import { Injectable } from '@angular/core';
import { CanActivate } from '@angular/router';

import { ErrorHandlerService } from './error-handler.service';
import { UserService } from './user.service';

@Injectable()
export class AuthenticationGuard implements CanActivate {
    constructor(private _userService: UserService, private _errorHandlerService: ErrorHandlerService) { }

    canActivate(): boolean {
        if (this._userService.loggedIn) {
            return true;
        }

        this._errorHandlerService.handleUnauthorizedError();
        return false;
    }
}