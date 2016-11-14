import { TestBed, inject } from '@angular/core/testing';

import { UserService } from './user.service';
import { StorageService } from './storage.service';

let testUsername = `testUsername`;

describe(`UserService`, () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [UserService, { provide: StorageService, useClass: StubStorageService }]
        });
    });

    describe(`syncWithStorage`, () => {
        it(`Sets username and sets loggedIn to true if username exists in storage`,
            inject([UserService, StorageService], (userService: UserService, storageService: StubStorageService) => {
                spyOn(storageService, `getItem`).and.returnValue(testUsername);

                userService.syncWithStorage();

                expect(storageService.getItem).toHaveBeenCalledWith(userService.storageName);
                expect(userService.loggedIn).toBe(true);
                expect(userService.username).toBe(testUsername);
            })
        );

        it(`Sets username to falsey value and sets loggedIn to false if username does not exist in storage`,
            inject([UserService, StorageService], (userService: UserService, storageService: StubStorageService) => {
                spyOn(storageService, `getItem`).and.returnValue(undefined);

                userService.syncWithStorage();

                expect(storageService.getItem).toHaveBeenCalledWith(userService.storageName);
                expect(userService.loggedIn).toBe(false);
                expect(userService.username).toBe(undefined);
            })
        );
    });

    it(`logIn sets username and sets loggedIn to true. Saves username in storage.`,
        inject([UserService, StorageService], (userService: UserService, storageService: StubStorageService) => {
            spyOn(storageService, `setItem`);

            userService.logIn(testUsername, true);

            expect(storageService.setItem).toHaveBeenCalledWith(userService.storageName, testUsername, true);
            expect(userService.loggedIn).toBe(true);
            expect(userService.username).toBe(testUsername);
        })
    );

    it(`logOff sets username to falsey value and sets loggedIn to false. Removes username from storage.`,
        inject([UserService, StorageService], (userService: UserService, storageService: StubStorageService) => {
            spyOn(storageService, `removeItem`);

            userService.logOff();

            expect(storageService.removeItem).toHaveBeenCalledWith(userService.storageName);
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
