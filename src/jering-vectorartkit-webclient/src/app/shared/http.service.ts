import { Injectable } from '@angular/core';
import { Http, Headers, RequestOptions, RequestOptionsArgs, Response, RequestMethod } from '@angular/http';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';

import { environment } from '../../environments/environment';
import { ErrorHandlerService } from './utility/error-handler.service';
import { ErrorResponseModel } from './response-models/error.response-model';

/**
 * Provides Http helpers
 */
@Injectable()
export class HttpService {
    constructor(private _http: Http, private _errorHandlerService: ErrorHandlerService) { }

    /**
     * Sends post request
     *
     * Returns
     * - Observable<ResponseModel> if request succeeds
     * - ErrorObservable<ErrorResponseModel> or empty Observable if request fails
     */
    post(relativeUrl: string, body: any, options?: RequestOptionsArgs, domain?: string): Observable<any> {
        options = options || new RequestOptions();
        // Note that Http.request assigns content-type based on body's type
        options.body = body;
        options.method = RequestMethod.Post;

        return this.request(relativeUrl, options, domain);
    }

    /**
     * Sends get request
     *
     * Returns
     * - Observable<ResponseModel> if request succeeds
     * - ErrorObservable<ErrorResponseModel> or empty Observable if request fails
     */
    get(relativeUrl: string, options?: RequestOptionsArgs, domain?: string): Observable<any> {
        options = options || new RequestOptions();
        options.method = RequestMethod.Get;

        return this.request(relativeUrl, options, domain);
    }

    /**
     * Sends request of any method
     *
     * Returns
     * - Observable<ResponseModel> if request succeeds
     * - ErrorObservable<ErrorResponseModel> or empty Observable if request fails
     */
    request(relativeUrl: string, options: RequestOptionsArgs, domain?: string): Observable<any> {
        if (!options.headers) {
            options.headers = new Headers();
        }

        // Required to prevent AspNetCore redirecting on failed authentication
        options.headers.append(`X-Requested-With`, `XMLHttpRequest`);
        // Required by AspNetCore authentication (sends cookies with XMLHttpRequest)
        options.withCredentials = true;

        return this.
            _http.
            request(this.urlFromRelativeUrl(relativeUrl, domain), options).
                    // retry if request fails and error is not expected
            //retryWhen().
            map(response => response.json()).
            catch(error => this.handleRequestError(error));
    }

    /**
     * Handles a request error. Passes error on to error Observer if error is expected. Otherwise,
     * passes error to ErrorHandlerService.
     *
     * Returns
     * - ErrorObservable<ErrorResponseModel> with expectedError set to true if error is expected
     * - Otherwise, empty Observable.   
     */
    private handleRequestError(error: any): Observable<any> {
        let body: ErrorResponseModel;

        if (error instanceof Response &&
            (body = error.json()) &&
            body.expectedError) {

            // Pass expected errors on to error observer
            return Observable.throw(body);
        }

        // Pass unexpected errors on to ErrorHandlerService
        this._errorHandlerService.handleUnexpectedError(error);
        return Observable.empty();
    }

    private urlFromRelativeUrl(relativeUrl: string, domain?: string): string {
        return `${domain ? domain : environment.apiDomain}${relativeUrl}`;
    }
}