import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { HttpService } from 'app/shared/http.service';
import { SetAltEmailVerifiedResponseModel } from 'api/response-models/set-alt-email-verified.response-model';
import { SetAltEmailVerifiedRequestModel } from 'api/request-models/set-alt-email-verified.request-model';
import { AccountControllerRelativeUrls } from 'api/api-relative-urls/account-controller.relative-urls';

@Injectable()
export class VerifyAltEmailGuard implements Resolve<SetAltEmailVerifiedResponseModel> {
    constructor(private _httpService: HttpService) { }

    resolve(route: ActivatedRouteSnapshot): Observable<SetAltEmailVerifiedResponseModel>{
        let requestModel: SetAltEmailVerifiedRequestModel = {
            token: route.params[`token`]
        };

        return this._httpService.
            post(AccountControllerRelativeUrls.setAltEmailVerified, requestModel ).
            catch(responseModel => Observable.of(responseModel));;
    };
}
