import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { HttpService } from 'app/shared/http.service';
import { UserService } from 'app/shared/user.service';
import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';
import { AppPaths } from 'app/app.paths';

@Component({
    selector: 'app',
    templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
    private _logOffRelativeUrl = AccountControllerRelativeUrls.logOff;
    title = 'Vector Art Kit';

    homePath = AppPaths.homePath;
    signUpPath = AppPaths.signUpPath;
    logInPath = AppPaths.logInPath;
    manageAccountPath = AppPaths.manageAccountPath;

    constructor(public userService: UserService,
        private _httpService: HttpService,
        private _router: Router) { }

    ngOnInit(): void {
        this._httpService.init();
        this.userService.init();
    }

    logOff(): void {
        this.
            _httpService.
            post(this._logOffRelativeUrl, null).
            subscribe(responseModel => {
                this._router.navigate([this.homePath]);
            },
            undefined,
            () => {
                this.userService.logOff();
            });
    }
}
