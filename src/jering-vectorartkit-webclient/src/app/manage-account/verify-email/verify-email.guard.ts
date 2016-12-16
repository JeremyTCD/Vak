import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { HttpService } from '../../shared/http.service';
import { SetEmailVerifiedResponseModel } from '../../shared/response-models/set-email-verified.response-model';

@Injectable()
export class VerifyEmailGuard implements Resolve<SetEmailVerifiedResponseModel> {
    constructor(private _httpService: HttpService) { }

    resolve(route: ActivatedRouteSnapshot): Observable<SetEmailVerifiedResponseModel>{
        return this._httpService.
            post(`Account/SetEmailVerified`, {
                accountid: route.params[`accountid`],
                token: route.params[`token`]
            }).
            catch(responseModel => Observable.of(responseModel));;
    };
}
