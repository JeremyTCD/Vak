import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { HttpService } from './shared/http.service';
import { UserService } from './shared/user.service';

@Component({
    selector: 'app',
    templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
    private _logOffRelativeUrl = `Account/LogOff`;
    title = 'Vector Art Kit';

    constructor(public userService: UserService,
        private _httpService: HttpService,
        private _router: Router) { }

    ngOnInit(): void {
        this.userService.init();
    }

    logOff(): void {
        this.
            _httpService.
            post(this._logOffRelativeUrl, null).
            subscribe(responseModel => {
                this._router.navigate([`/home`]);
            },
            undefined,
            () => {
                this.userService.logOff();
            });
    }
}
