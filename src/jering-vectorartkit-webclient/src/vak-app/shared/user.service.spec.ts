import { TestBed, inject } from '@angular/core/testing';
import { RequestOptionsArgs, Response, ResponseOptions } from '@angular/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';

import { UserService } from './user.service';
import { StorageService } from './storage.service';
import { HttpService } from './http.service';
import { StubRouter } from '../../testing/router-stubs';

let testUsername = `testUsername`;
let testStorageName = `vakUsername`;
let testLogOffRelativeUrl = `Account/LogOff`;
let testResponse = new Response(
    new ResponseOptions({
    })
);

describe(`UserService`, () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [UserService,
                { provide: StorageService, useClass: StubStorageService },
                { provide: HttpService, useClass: StubHttpService },
                { provide: Router, useClass: StubRouter }]
        });
    });

    describe(`syncWithStorage`, () => {
        it(`Sets username and sets loggedIn to true if username exists in storage`,
            inject([UserService, StorageService], (userService: UserService, stubStorageService: StubStorageService) => {
                spyOn(stubStorageService, `getItem`).and.returnValue(testUsername);

                userService.syncWithStorage();

                expect(stubStorageService.getItem).toHaveBeenCalledWith(testStorageName);
                expect(userService.loggedIn).toBe(true);
                expect(userService.username).toBe(testUsername);
            })
        );

        it(`Sets username to falsey value and sets loggedIn to false if username does not exist in storage`,
            inject([UserService, StorageService], (userService: UserService, stubStorageService: StubStorageService) => {
                spyOn(stubStorageService, `getItem`).and.returnValue(undefined);

                userService.syncWithStorage();

                expect(stubStorageService.getItem).toHaveBeenCalledWith(testStorageName);
                expect(userService.loggedIn).toBe(false);
                expect(userService.username).toBe(undefined);
            })
        );
    });

    it(`logIn sets username, sets loggedIn to true and saves username in storage.`,
        inject([UserService, StorageService], (userService: UserService, stubStorageService: StubStorageService) => {
            spyOn(stubStorageService, `setItem`);

            userService.logIn(testUsername, true);

            expect(stubStorageService.setItem).toHaveBeenCalledWith(testStorageName, testUsername, true);
            expect(userService.loggedIn).toBe(true);
            expect(userService.username).toBe(testUsername);
        })
    );

    it(`logOff sets username to null, sets loggedIn to false and removes username from storage. 
        Sends post request to log off and navigates to home if log off is successful.`,
        inject([UserService, StorageService, HttpService, Router], (userService: UserService,
            stubStorageService: StubStorageService,
            stubHttpService: StubHttpService,
            stubRouter: StubRouter) => {
            spyOn(stubStorageService, `removeItem`);
            spyOn(stubHttpService, `post`).and.returnValue(Observable.of(testResponse));
            spyOn(stubRouter, `navigate`);

            userService.logOff();

            expect(stubStorageService.removeItem).toHaveBeenCalledWith(testStorageName);
            expect(userService.loggedIn).toBe(false);
            expect(userService.username).toBe(null);
            expect(stubHttpService.post).toHaveBeenCalledWith(testLogOffRelativeUrl, null);
            expect(stubRouter.navigate).toHaveBeenCalledWith([`/home`]);
        })
    );
});

class StubHttpService {
    post(url: string, body: any, options?: RequestOptionsArgs): Observable<Response> {
        return Observable.of(testResponse);
    }
}

class StubStorageService {
    setItem(key: string, data: string, isPersistent: boolean): void {
    }

    getItem(key: string): string {
        return null;
    }

    removeItem(key: string): void {
    }
}
