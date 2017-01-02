import { Injectable, OnDestroy } from '@angular/core';
import { Http, Headers, RequestOptions, RequestOptionsArgs, Response, RequestMethod, Request } from '@angular/http';
import { Observable, Subscription, ConnectableObservable } from 'rxjs';

import { environment } from 'environments/environment';
import { ErrorHandlerService } from './error-handler.service';

import { ErrorResponseModel } from 'api/response-models/error.response-model';
import { AntiForgeryControllerRelativeUrls } from 'api/api-relative-urls/anti-forgery-controller.relative-urls';

import { CookieService } from 'app/shared/cookie.service';

/**
 * Provides Http helpers
 */
@Injectable()
export class HttpService implements OnDestroy {
    private _afTokensAvailable: boolean = false;
    private _numRetries: number = 3;
    private _getAfTokensSubscription: Subscription;
    private _pendingPosts: Array<ConnectableObservable<Response>> = new Array<ConnectableObservable<Response>>();
    private _requestTokenCookieName: string = `XSRF-TOKEN`;

    constructor(private _http: Http,
        private _errorHandlerService: ErrorHandlerService,
        private _cookieService: CookieService) { }

    init() {
        if (this._cookieService.cookieExists(this._requestTokenCookieName)){
            this._afTokensAvailable = true;
        } else {
            this._getAfTokensSubscription = this.
                get(AntiForgeryControllerRelativeUrls.getAntiForgeryTokens, null).
                subscribe(result => {
                    this._afTokensAvailable = true;
                    for (let observable of this._pendingPosts) {
                        observable.connect();
                    }
                    this._pendingPosts = [];
                });
        }
    }

    /**
     * Constructs an Observable that executes a post request. If anti-forgery tokens are not available,
     * pauses Observable until they are.
     *
     * Returns
     * - Observable<ResponseModel>
     */
    post(relativeUrl: string, body: { [key: string]: any }, options?: RequestOptions, domain?: string): Observable<any> {
        options = options || new RequestOptions();
        // Note that Http.request assigns content-type based on body's type
        options.body = body;
        options.method = RequestMethod.Post;
        options.url = this.urlFromRelativeUrl(relativeUrl, domain);

        let result: Observable<any> = this.
            request(options);

        if (this._afTokensAvailable) {
            return result;
        }

        let connectableResult: ConnectableObservable<any> = result.publish();
        this._pendingPosts.push(connectableResult);

        return connectableResult;
    }

    /**
     * Constructs an Observable that executes a set request.
     *
     * Returns
     * - Observable<ResponseModel>
     */
    get(relativeUrl: string, options?: RequestOptions, domain?: string): Observable<any> {
        options = options || new RequestOptions();
        options.method = RequestMethod.Get;
        options.url = this.urlFromRelativeUrl(relativeUrl, domain);

        return this.
            request(options);
    }

    /**
     * Constructs an Observable that executes a request of any method.
     *
     * Returns
     * - Observable<ResponseModel>
     */
    request(options: RequestOptions): Observable<any> {
        if (!options.headers) {
            options.headers = new Headers();
        }

        // Required to prevent AspNetCore redirecting on failed auth
        options.headers.append(`X-Requested-With`, `XMLHttpRequest`);
        // Required by AspNetCore auth (sends cookies with XMLHttpRequest)
        options.withCredentials = true;

        return Observable.
            of(options).
            map(options => this._http.request(new Request(options))).
            switch().
            retryWhen((errors: Observable<Response>) => this.handleRetry(errors)).
            map(this.responseModelFromResponse, this).
            catch((error: any) => this.handleRequestError(error));
    }

    /**
     * If an unexpected error or anti-forgery error occurs, retry up to _numRetries times.
     */
    private handleRetry(errors: Observable<Response>): Observable<number> {
        return errors.
            // Don't retry expected errors (other than anti-forgery errors)
            do((error: any) => {
                let responseModel: ErrorResponseModel = this.responseModelFromError(error);

                if (responseModel &&
                    responseModel.expectedError &&
                    !responseModel.antiForgeryError) {
                    throw error;
                }
            }).
            delay(100).
            // Retry up to _numRetries times
            scan((errorCount: number, error: Response) => {
                if (errorCount >= this._numRetries) {
                    throw error;
                }
                return errorCount + 1;
            }, 0);
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
        let responseModel: ErrorResponseModel = this.responseModelFromError(error);

        if (responseModel && responseModel.expectedError) {
            if (responseModel.authenticationError) {
                this._errorHandlerService.handleUnauthorizedError();

                return Observable.empty();
            }

            // Pass expected errors on to error observer
            return Observable.throw(responseModel);
        }

        this._errorHandlerService.handleUnexpectedError(responseModel || error);

        return Observable.empty();
    }

    /**
     * Attempts to extract response model from error object.
     *
     * Returns
     * - ErrorResponseModel if successful.
     * - null otherwise.
     */
    private responseModelFromError(error: any) {
        if (error instanceof Response) {
            return this.responseModelFromResponse(error);
        }

        return null;
    }

    private urlFromRelativeUrl(relativeUrl: string, domain?: string): string {
        return `${domain ? domain : environment.apiDomain}${relativeUrl}`;
    }

    private responseModelFromResponse(response: Response) {
        try {
            // Throws SyntaxError if body is not parsable json
            return response.json();
        } catch (error) {
            return undefined;
        }
    }

    ngOnDestroy(): void {
        this._getAfTokensSubscription.unsubscribe();
    }
}