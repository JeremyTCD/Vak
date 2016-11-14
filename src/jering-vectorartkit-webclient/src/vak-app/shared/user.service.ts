import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

/**
 * Provides error handling
 */
@Injectable()
export class UserService {
    userName: string = null;
    loggedIn: boolean = false;

    logIn(userName: string): void {
        this.userName = userName;
        this.loggedIn = true;
    }

    logOff(): void {
        this.userName = null;
        this.loggedIn = false;
    }
}