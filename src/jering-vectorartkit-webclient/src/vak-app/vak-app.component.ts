import { Component, OnInit } from '@angular/core';

import { UserService } from './shared/user.service';

@Component({
    selector: 'vak-app',
    templateUrl: './vak-app.component.html'
})
export class VakAppComponent implements OnInit {
    title = 'Vector Art Kit';

    constructor(public userService: UserService) { }

    ngOnInit(): void {
        this.userService.syncWithStorage();
    }
}
