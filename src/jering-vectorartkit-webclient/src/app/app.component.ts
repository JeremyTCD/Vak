import { Component, OnInit } from '@angular/core';

import { UserService } from './shared/user.service';

@Component({
    selector: 'app',
    templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
    title = 'Vector Art Kit';

    constructor(public userService: UserService) { }

    ngOnInit(): void {
        this.userService.syncWithStorage();
    }

    logOff(): void {
        this.userService.logOff();
    }
}
