import { Injectable } from '@angular/core';
import { Headers, RequestOptions } from '@angular/http';
import { Subject, Observable } from 'rxjs';
import { Router } from '@angular/router';

import { HttpService } from './http.service';
import { StorageService } from './storage.service';

/**
 * Provides user authentication state and management
 */
@Injectable()
export class UserService {
    private _logOffRelativeUrl = `Account/LogOff`;
    private _storageName = `vakUsername`;

    username: string;
    loggedIn: boolean;

    constructor(private _storageService: StorageService,
        private _httpService: HttpService,
        private _router: Router) { }

    /**
     * Sets username and sets loggedIn to true if username exists in local/session storage.
     * Otherwise, sets loggedIn to false and username to a falsey value.
     */
    syncWithStorage(): void {
        this.username = this._storageService.getItem(this._storageName);
        if (this.username) {
            this.loggedIn = true;
        } else {
            this.loggedIn = false;
        }
    }

    /**
     * Sets username, sets loggedIn to true and saves username in local/session storage
     */
    logIn(username: string, isPersistent: boolean): void {
        this.username = username;
        this._storageService.setItem(this._storageName, username, isPersistent);
        this.loggedIn = true;
    }

    /**
     * Sets username to null, sets loggedIn to false and removes username from local/session storage.
     * Sends post request to log off.
     */
    logOff(): void {
        this.
            _httpService.
            post(this._logOffRelativeUrl, null).
            subscribe(responseModel => {
                this._router.navigate([`/home`]);
            },
            undefined,
            () => {
                this.username = null;
                this._storageService.removeItem(this._storageName);
                this.loggedIn = false;
            });
    }
}