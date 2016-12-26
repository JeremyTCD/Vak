import { Injectable } from '@angular/core';
import { Headers, RequestOptions } from '@angular/http';
import { Subject, Observable } from 'rxjs';

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
    isPersistent: boolean;

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
     * Sets username, sets loggedIn to true and saves username in local/session storage
     */
    logIn(username: string, isPersistent: boolean): void {
        this.username = username;
        this.isPersistent = isPersistent;
        this._storageService.setItem(this._storageName, username, isPersistent);
        this.loggedIn = true;
    }

    logOff() {
        this.username = null;
        this._storageService.removeItem(this._storageName);
        this.loggedIn = false;
    }

    changeUsername(username: string) {
        this.username = username;
        this._storageService.setItem(this._storageName, username, this.isPersistent);
    }
}