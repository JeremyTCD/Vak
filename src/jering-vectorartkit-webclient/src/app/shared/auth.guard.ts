import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild } from '@angular/router';

import { ErrorHandlerService } from './error-handler.service';
import { UserService } from './user.service';

@Injectable()
export class AuthGuard implements CanActivate, CanActivateChild {
    constructor(private _userService: UserService, private _errorHandlerService: ErrorHandlerService) { }

    canActivate(): boolean {
        if (this._userService.loggedIn) {
            return true;
        }

        this._errorHandlerService.handleUnauthorizedError();
        return false;
    }

    canActivateChild(): boolean {
        return this.canActivate();
    }
}