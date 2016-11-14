import { Injectable } from '@angular/core';
import { Router, Resolve, ActivatedRouteSnapshot } from '@angular/router';

import { UserService } from './user.service';

@Injectable()
export class UserResolve implements Resolve<boolean> {
    constructor(private _userService: UserService) { }

    resolve(route: ActivatedRouteSnapshot): boolean {
        let id = +route.params['id'];
        return true;//TODO this._userService

            //.getCrisis(id).then(crisis => {
            //if (crisis) {
            //    return crisis;
            //} else { // id not found
            //    this.router.navigate(['/crisis-center']);
            //    return false;
            //}
            //});
    }
}
