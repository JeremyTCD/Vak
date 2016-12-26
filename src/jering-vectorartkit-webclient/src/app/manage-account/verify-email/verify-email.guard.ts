import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { HttpService } from 'app/shared/http.service';
import { SetEmailVerifiedResponseModel } from 'api/response-models/set-email-verified.response-model';
import { SetEmailVerifiedRequestModel } from 'api/request-models/set-email-verified.request-model';
import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';

@Injectable()
export class VerifyEmailGuard implements Resolve<SetEmailVerifiedResponseModel> {
    constructor(private _httpService: HttpService) { }

    resolve(route: ActivatedRouteSnapshot): Observable<SetEmailVerifiedResponseModel>{
        let requestModel: SetEmailVerifiedRequestModel = {
            token: route.params[`token`]
        };

        return this._httpService.
            post(AccountControllerRelativeUrls.setEmailVerified, requestModel).
            catch(responseModel => Observable.of(responseModel));;
    };
}
