import { Injectable } from '@angular/core';
import { Resolve, Router } from '@angular/router';
import { Observable } from 'rxjs';

import { GetAccountDetailsResponseModel } from '../shared/response-models/get-account-details.response-model';
import { HttpService } from '../shared/http.service';

@Injectable()
export class ManageAccountGuard implements Resolve<GetAccountDetailsResponseModel> {
    constructor(private _httpService: HttpService, private _router: Router) { }

    resolve(): Observable<GetAccountDetailsResponseModel>{
        let returnUrl = this._router.routerState.snapshot.url;
        return this._httpService.get(`Account/GetAccountDetails`);
    };
}
