import { TestBed, inject } from '@angular/core/testing';
import { RequestOptionsArgs, Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';

import { UserService } from './user.service';
import { StorageService } from './storage.service';
import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';

let testUsername = `testUsername`;
let testStorageName = `vakUsername`;
let testLogOffRelativeUrl = AccountControllerRelativeUrls.logOff;
let testResponse = new Response(
    new ResponseOptions({
    })
);

describe(`UserService`, () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [UserService,
                { provide: StorageService, useClass: StubStorageService }]
        });
    });

    describe(`init`, () => {
        it(`Sets username and sets loggedIn to true if username exists in storage`,
            inject([UserService, StorageService], (userService: UserService, stubStorageService: StubStorageService) => {
                spyOn(stubStorageService, `getItem`).and.returnValue(testUsername);

                userService.init();

                expect(stubStorageService.getItem).toHaveBeenCalledWith(testStorageName);
                expect(userService.loggedIn).toBe(true);
                expect(userService.username).toBe(testUsername);
            })
        );

        it(`Sets username to falsey value and sets loggedIn to false if username does not exist in storage`,
            inject([UserService, StorageService], (userService: UserService, stubStorageService: StubStorageService) => {
                spyOn(stubStorageService, `getItem`).and.returnValue(undefined);

                userService.init();

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

    it(`logOff sets username to null, sets loggedIn to false and removes username from storage.`,
        inject([UserService, StorageService], (userService: UserService,
            stubStorageService: StubStorageService) => {
            spyOn(stubStorageService, `removeItem`);

            userService.logOff();

            expect(stubStorageService.removeItem).toHaveBeenCalledWith(testStorageName);
            expect(userService.loggedIn).toBe(false);
            expect(userService.username).toBe(null);
        })
    );
});

class StubStorageService {
    setItem(key: string, data: string, isPersistent: boolean): void {
    }

    getItem(key: string): string {
        return null;
    }

    removeItem(key: string): void {
    }
}
