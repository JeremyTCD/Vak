import { Http, RequestOptionsArgs, Response, ResponseOptions, RequestMethod } from '@angular/http';
import { TestBed, inject } from '@angular/core/testing';
import { Observable } from 'rxjs';

import { HttpService } from './http.service';
import { ErrorHandlerService } from './utility/error-handler.service';
import { environment } from '../../environments/environment';
import { ErrorResponseModel } from './response-models/error.response-model';

let testRelativeUrl = `testRelativeUrl`;
let testUrl = `${environment.apiDomain}${testRelativeUrl}`;
let testRequestBody = {};
let stubResponseModel: StubResponseModel = { testProperty: `testProperty` };

describe(`HttpService`, () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [HttpService,
                { provide: ErrorHandlerService, useClass: StubErrorHandlerService },
                { provide: Http, useClass: StubHttp }]
        });
    });

    it(`post creates and prepares RequestOptions for a post request and calls request with prepared options`,
        inject([HttpService], (httpService: HttpService) => {
            let spy = spyOn(httpService, `request`);

            httpService.post(testRelativeUrl, testRequestBody);

            let args = spy.calls.first().args;
            expect(args[0]).toBe(testRelativeUrl);
            let requestOptionsArgs = args[1] as RequestOptionsArgs;
            expect(requestOptionsArgs.method).toBe(RequestMethod.Post);
            expect(requestOptionsArgs.body).toBe(testRequestBody);
        })
    );

    it(`get creates and prepares RequestOptions for a get request and calls request with prepared options`,
        inject([HttpService], (httpService: HttpService) => {
            let spy = spyOn(httpService, `request`);

            httpService.get(testRelativeUrl);

            let args = spy.calls.first().args;
            expect(args[0]).toBe(testRelativeUrl);
            let requestOptionsArgs = args[1] as RequestOptionsArgs;
            expect(requestOptionsArgs.method).toBe(RequestMethod.Get);
        })
    );

    describe(`request`, () => {
        let testResponse: Response;

        it(`Prepares RequestOptions for a request and calls Http.request`,
            inject([HttpService, Http], (httpService: HttpService, http: StubHttp) => {
                testResponse = new Response(new ResponseOptions({}));
                let spy = spyOn(http, `request`).and.returnValue(Observable.of(testResponse));

                httpService.request(testRelativeUrl, {});

                let args = spy.calls.first().args;
                expect(args[0]).toBe(testUrl);
                let requestOptionsArgs = args[1] as RequestOptionsArgs;
                expect(requestOptionsArgs.withCredentials).toBe(true);
                expect(requestOptionsArgs.headers.get(`X-Requested-With`)).toBe(`XMLHttpRequest`);
            })
        );

        it(`Returns deserialized body of Response if request succeeds`,
            inject([HttpService, Http], (httpService: HttpService, http: StubHttp) => {
                testResponse = new Response(new ResponseOptions({ body: JSON.stringify(stubResponseModel) }));
                spyOn(http, `request`).and.returnValue(Observable.of(testResponse));
                let result: StubResponseModel;

                httpService.
                    request(testRelativeUrl, {}).
                    subscribe(responseModel => result = responseModel);

                expect(result).toEqual(stubResponseModel);
            })
        );

        describe(`If request fails`, () => {
            let errorResponseModel: ErrorResponseModel;
            let nextOrErrorCalled: boolean;
            let completeCalled: boolean;
            let testObserver = {
                    next: next => nextOrErrorCalled = true,
                    error: error => nextOrErrorCalled,
                    complete: () => completeCalled = true
            };

            beforeEach(() => {
                nextOrErrorCalled = false;
                completeCalled = false;
            });

            it(`Returns deserialized body of Response if error is expected`,
                inject([HttpService, Http], (httpService: HttpService, http: StubHttp) => {
                    errorResponseModel = { expectedError: true };
                    testResponse = new Response(new ResponseOptions({ body: JSON.stringify(errorResponseModel) }));
                    spyOn(http, `request`).and.returnValue(Observable.of(testResponse));
                    let result: StubResponseModel;

                    httpService.
                        request(testRelativeUrl, {}).
                        subscribe(responseModel => result = responseModel);

                    expect(result).toEqual(errorResponseModel);
                })
            );

            it(`Calls ErrorHandlerService.handleUnexpectedError if error is unexpected. Calls complete
                observer immediately, does not call next or error observer.`,
                inject([HttpService, Http, ErrorHandlerService], (httpService: HttpService, http: StubHttp, errorHandlerService: StubErrorHandlerService) => {
                    errorResponseModel = { expectedError: false };
                    testResponse = new Response(new ResponseOptions({ body: JSON.stringify(errorResponseModel) }));
                    spyOn(errorHandlerService, `handleUnexpectedError`);
                    spyOn(http, `request`).and.returnValue(Observable.throw(testResponse));

                    httpService.
                        request(testRelativeUrl, {}).
                        subscribe(testObserver);

                    expect(nextOrErrorCalled).toBe(false);
                    expect(completeCalled).toBe(true);
                    expect(errorHandlerService.handleUnexpectedError).toHaveBeenCalledWith(testResponse);
                })
            );

            it(`Calls ErrorHandlerService.handleUnexpectedError if error is not a response. Calls complete
                observer immediately, does not call next or error observer.`,
                inject([HttpService, Http, ErrorHandlerService], (httpService: HttpService, http: StubHttp, errorHandlerService: StubErrorHandlerService) => {
                    let testErrorObject = {};
                    spyOn(errorHandlerService, `handleUnexpectedError`);
                    spyOn(http, `request`).and.returnValue(Observable.throw(testErrorObject));

                    httpService.
                        request(testRelativeUrl, {}).
                        subscribe(testObserver);

                    expect(nextOrErrorCalled).toBe(false);
                    expect(completeCalled).toBe(true);
                    expect(errorHandlerService.handleUnexpectedError).toHaveBeenCalledWith(testErrorObject );
                })
            );

            it(`Calls ErrorHandlerService.handleUnexpectedError if error does not have a body in json format`,
                inject([HttpService, Http, ErrorHandlerService], (httpService: HttpService, http: StubHttp, errorHandlerService: StubErrorHandlerService) => {
                    testResponse = new Response(new ResponseOptions({ body: null }));
                    spyOn(http, `request`).and.returnValue(Observable.throw(testResponse));
                    spyOn(errorHandlerService, `handleUnexpectedError`);

                    httpService.
                        request(testRelativeUrl, {}).
                        subscribe(testObserver);

                    expect(nextOrErrorCalled).toBe(false);
                    expect(completeCalled).toBe(true);
                    expect(errorHandlerService.handleUnexpectedError).toHaveBeenCalledWith(testResponse);
                })
            );
        });
    });
});

interface StubResponseModel {
    testProperty: string;
}

class StubErrorHandlerService {
    handleUnexpectedError(error: any): void {
    }
}

class StubHttp {
    get(url: string, options?: RequestOptionsArgs, domain?: string): Observable<Response> {
        return new Observable<Response>();
    };

    post(url: string, body: string, options?: RequestOptionsArgs, domain?: string): Observable<Response> {
        return new Observable<Response>();
    };

    request(url: string, options?: RequestOptionsArgs): Observable<Response> {
        return new Observable<Response>();
    }
}
