import { Injectable } from '@angular/core';
import { Headers, RequestOptions } from '@angular/http';

import { HttpService } from './http.service';
import { StorageService } from './storage.service';

/**
 * Provides user auth state and management
 */
@Injectable()
export class UserService {
    private _storageName = `vakUsername`;

    username: string;
    loggedIn: boolean;

    constructor(private _storageService: StorageService) { }

    /**
     * Sets username and sets loggedIn to true if username exists in local/session storage.
     * Otherwise, sets loggedIn to false and username to a falsey value.
     */
    init(): void {
        this.username = this._storageService.getItem(this._storageName);
        if (this.username) {
            this.loggedIn = true;
        } else {
            this.loggedIn = false;
        }
    }

    /**
     * Sets username, sets loggedIn to true and saves username in local storage
     */
    logIn(username: string): void {
        this.username = username;
        this._storageService.setItem(this._storageName, username, true);
        this.loggedIn = true;
    }

    logOff() {
        this.username = null;
        this._storageService.removeItem(this._storageName);
        this.loggedIn = false;
    }

    changeUsername(username: string) {
        this.username = username;
        this._storageService.setItem(this._storageName, username, true);
    }
}