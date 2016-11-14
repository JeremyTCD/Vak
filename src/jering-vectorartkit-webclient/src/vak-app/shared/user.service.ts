import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

import { StorageService } from './storage.service';

/**
 * Provides user authentication state and management
 */
@Injectable()
export class UserService {
    storageName = `vakUsername`;
    username: string = null;
    loggedIn: boolean;

    constructor(private _storageService: StorageService) { }

    /**
     * Sets username and sets loggedIn to true if username exists in local/session storage.
     * Otherwise, sets loggedIn to false and username to a falsey value.
     */
    syncWithStorage(): void {
        this.username = this._storageService.getItem(this.storageName);
        if (this.username) {
            this.loggedIn = true;
        } else {
            this.loggedIn = false;
        }
    }

    /**
     * Sets username and sets loggedIn to true. Saves username in local/session storage
     */
    logIn(username: string, isPersistent: boolean): void {
        this.username = username;
        this._storageService.setItem(this.storageName, username, isPersistent);
        this.loggedIn = true;
    }

    /**
     * Sets username to null and sets loggedIn to false. Removes username from local/session storage
     */
    logOff(): void {
        this.username = null;
        this._storageService.removeItem(this.storageName);
        this.loggedIn = false;
    }
}