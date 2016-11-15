import { TestBed, inject } from '@angular/core/testing';
import { Response, ResponseOptions, RequestOptionsArgs, URLSearchParams } from '@angular/http';
import { Observable } from 'rxjs/Observable';

import { DynamicControl } from './dynamic-control/dynamic-control';
import { DynamicForm } from './dynamic-form/dynamic-form';
import { DynamicFormsService } from './dynamic-forms.service';
import { ErrorHandlerService } from '../utility/error-handler.service';
import { environment } from '../../../environments/environment';
import { Validity } from './validity';
import { HttpService } from '../http.service';

let testFormModelName = `testFormModelName`;
let testControlName = `testControlName`;
let testMessage = `testMessage`;
let testRelativeUrl = `testRelativeUrl`;
let testDynamicControl: DynamicControl<any>;
let testDynamicForm: DynamicForm;
let testResponse: Response;

describe('DynamicFormsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [DynamicFormsService, { provide: ErrorHandlerService, useClass: StubErrorHandlerService }, { provide: HttpService, useClass: StubHttpService }]
        });
        testDynamicControl = new DynamicControl<any>({ name: testControlName });
        testDynamicForm = new DynamicForm([testDynamicControl], testMessage);
    });

    describe(`getDynamicForm`, () => {
        beforeEach(() => {
            testResponse = new Response(
                new ResponseOptions({
                    body: `{"dynamicControlDatas":{}}`
                })
            );
        });

        it('Calls Http.get',
            inject([DynamicFormsService, HttpService], (dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                let getSpy = spyOn(stubHttpService, `get`).and.returnValue(Observable.of(testResponse));

                dynamicFormsService.
                    getDynamicForm(testFormModelName).
                    subscribe(dynamicForm => null);

                expect(stubHttpService.get).toHaveBeenCalledTimes(1);
                let args = getSpy.calls.first().args;
                expect(args[0]).toBe(`DynamicForms/GetDynamicForm`);
                let requestOptions = args[1] as RequestOptionsArgs;
                let urlSearchParams = requestOptions.search as URLSearchParams;
                expect(urlSearchParams.get(`formModelName`)).toBe(testFormModelName);
            })
        );

        it(`Maps response to a DynamicForm if Http.get succeeds`,
            inject([DynamicFormsService, HttpService], (dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                spyOn(stubHttpService, `get`).and.returnValue(Observable.of(testResponse));
                let result: DynamicForm;

                dynamicFormsService.
                    getDynamicForm(testFormModelName).
                    subscribe(dynamicForm => result = dynamicForm);

                expect(result instanceof DynamicForm).toBe(true);
            })
        );

        it(`Calls ErrorHandlerService.handleUnexpectedError and calls neither next observer nor error observer if Http.get fails`,
            inject([ErrorHandlerService, DynamicFormsService, HttpService], (errorHandlerService: ErrorHandlerService,
                dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                let observer = {
                    next: (dynamicForm: DynamicForm) => null,
                    error: (error: any) => null,
                }
                spyOn(stubHttpService, `get`).and.returnValue(Observable.throw(testResponse));
                spyOn(errorHandlerService, `handleUnexpectedError`);
                spyOn(observer, `next`);
                spyOn(observer, `error`);

                dynamicFormsService.
                    getDynamicForm(testFormModelName).
                    subscribe(observer);

                expect(errorHandlerService.handleUnexpectedError).toHaveBeenCalledTimes(1);
                expect(observer.next).not.toHaveBeenCalled();
                expect(observer.error).not.toHaveBeenCalled();
            })
        );
    });

    describe(`submitDynamicForm`, () => {
        let modelState: { [key: string]: string[] };
        let observer: any;

        beforeEach(() => {
            modelState = { testControl: [`testError`] };
            observer = {
                next: (response: Response) => null,
                error: (error: any) => null,
            }
            testResponse = new Response(
                new ResponseOptions({})
            );
        });

        it(`Calls Http.post`, inject([DynamicFormsService, HttpService], (dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
            let getSpy = spyOn(stubHttpService, `post`).and.returnValue(Observable.of(testResponse));

            dynamicFormsService.
                submitDynamicForm(testRelativeUrl, testDynamicForm).
                subscribe(response => null);

            expect(stubHttpService.post).toHaveBeenCalledTimes(1);
            let args = getSpy.calls.first().args;
            expect(args[0]).toBe(testRelativeUrl);
            expect(args[1]).toEqual(testDynamicForm.value);
        })
        );

        it(`Calls error observer with modelState as error object if Http.post fails and returns a bad 
            request response with a json body that has modelState as a key`,
            inject([ErrorHandlerService, DynamicFormsService, HttpService], (errorHandlerService: ErrorHandlerService,
                dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                let response = new Response(
                    new ResponseOptions({
                        status: 400,
                        body: JSON.stringify({ modelState: modelState })
                    })
                );
                spyOn(stubHttpService, `post`).and.returnValue(Observable.throw(response));
                let errorSpy = spyOn(observer, `error`);

                dynamicFormsService.
                    submitDynamicForm(testRelativeUrl, testDynamicForm).
                    subscribe(observer);

                expect(observer.error).toHaveBeenCalledWith(modelState);
            })
        );

        it(`Calls ErrorHandlerService.handleUnexpectedError and calls neither next observer nor error observer if Http.post fails
            and error object is not a response`,
            inject([ErrorHandlerService, DynamicFormsService, HttpService], (errorHandlerService: ErrorHandlerService,
                dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {

                spyOn(stubHttpService, `post`).and.returnValue(Observable.throw(null));
                spyOn(errorHandlerService, `handleUnexpectedError`);
                spyOn(observer, `next`);
                spyOn(observer, `error`);

                dynamicFormsService.
                    submitDynamicForm(testRelativeUrl, testDynamicForm).
                    subscribe(observer);

                expect(errorHandlerService.handleUnexpectedError).toHaveBeenCalledTimes(1);
                expect(observer.next).not.toHaveBeenCalled();
                expect(observer.error).not.toHaveBeenCalled();
            })
        );

        it(`Calls ErrorHandlerService.handleUnexpectedError and calls neither next observer nor error observer if Http.post fails
            and error object is a response with status code != 400`,
            inject([ErrorHandlerService, DynamicFormsService, HttpService], (errorHandlerService: ErrorHandlerService,
                dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {

                let response = new Response(
                    new ResponseOptions({
                        status: 200,
                        body: JSON.stringify({ modelState: modelState })
                    })
                );
                spyOn(stubHttpService, `post`).and.returnValue(Observable.throw(response));
                spyOn(errorHandlerService, `handleUnexpectedError`);
                spyOn(observer, `next`);
                spyOn(observer, `error`);

                dynamicFormsService.
                    submitDynamicForm(testRelativeUrl, testDynamicForm).
                    subscribe(observer);

                expect(errorHandlerService.handleUnexpectedError).toHaveBeenCalledTimes(1);
                expect(observer.next).not.toHaveBeenCalled();
                expect(observer.error).not.toHaveBeenCalled();
            })
        );

        it(`Calls ErrorHandlerService.handleUnexpectedError and calls neither next observer nor error observer if Http.post fails
            and error object has no body`,
            inject([ErrorHandlerService, DynamicFormsService, HttpService], (errorHandlerService: ErrorHandlerService,
                dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                let response = new Response(
                    new ResponseOptions({
                        status: 400
                    })
                );
                spyOn(stubHttpService, `post`).and.returnValue(Observable.throw(response));
                spyOn(errorHandlerService, `handleUnexpectedError`);
                spyOn(observer, `next`);
                spyOn(observer, `error`);

                dynamicFormsService.
                    submitDynamicForm(testRelativeUrl, testDynamicForm).
                    subscribe(observer);

                expect(errorHandlerService.handleUnexpectedError).toHaveBeenCalledTimes(1);
                expect(observer.next).not.toHaveBeenCalled();
                expect(observer.error).not.toHaveBeenCalled();
            })
        );

        it(`Calls ErrorHandlerService.handleUnexpectedError and calls neither next observer nor error observer if Http.post fails
            and error object body has no modelState key`,
            inject([ErrorHandlerService, DynamicFormsService, HttpService], (errorHandlerService: ErrorHandlerService,
                dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                let response = new Response(
                    new ResponseOptions({
                        status: 400,
                        body: JSON.stringify({ testKey: modelState })
                    })
                );
                spyOn(stubHttpService, `post`).and.returnValue(Observable.throw(response));
                spyOn(errorHandlerService, `handleUnexpectedError`);
                spyOn(observer, `next`);
                spyOn(observer, `error`);

                dynamicFormsService.
                    submitDynamicForm(testRelativeUrl, testDynamicForm).
                    subscribe(observer);

                expect(errorHandlerService.handleUnexpectedError).toHaveBeenCalledTimes(1);
                expect(observer.next).not.toHaveBeenCalled();
                expect(observer.error).not.toHaveBeenCalled();
            })
        );
    });

    describe(`validateValue`, () => {
        let testValue = `testValue`;

        beforeEach(() => {
            testResponse = new Response(
                new ResponseOptions({})
            );
        });

        it('Calls Http.get',
            inject([DynamicFormsService, HttpService], (dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                let getSpy = spyOn(stubHttpService, `get`).and.returnValue(Observable.of(testResponse));

                dynamicFormsService.
                    validateValue(testRelativeUrl, testValue).
                    subscribe(validity => null);

                expect(stubHttpService.get).toHaveBeenCalledTimes(1);
                let args = getSpy.calls.first().args;
                expect(args[0]).toBe(testRelativeUrl);
                let requestOptions = args[1] as RequestOptionsArgs;
                let urlSearchParams = requestOptions.search as URLSearchParams;
                expect(urlSearchParams.get(`value`)).toBe(testValue);
            })
        );

        it(`Maps response to a Validity value if Http.get succeeds`,
            inject([DynamicFormsService, HttpService], (dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                testResponse = new Response(
                    new ResponseOptions({
                        body: JSON.stringify({ valid: true })
                    })
                );
                spyOn(stubHttpService, `get`).and.returnValue(Observable.of(testResponse));
                let result: Validity;

                dynamicFormsService.
                    validateValue(testRelativeUrl, testValue).
                    subscribe(validity => result = validity);

                expect(result).toBe(Validity.valid);
            })
        );

        it(`Returns Validity.valid if an error occurs. Calls next observer and does not call error observer.`,
            inject([ErrorHandlerService, DynamicFormsService, HttpService], (errorHandlerService: ErrorHandlerService,
                dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                let observer = {
                    next: (validity: Validity) => null,
                    error: (error: any) => null,
                }
                spyOn(stubHttpService, `get`).and.returnValue(Observable.throw(testResponse));
                spyOn(observer, `next`);
                spyOn(observer, `error`);

                dynamicFormsService.
                    validateValue(testRelativeUrl, testValue).
                    subscribe(observer);

                expect(observer.next).toHaveBeenCalledWith(Validity.valid);
                expect(observer.error).not.toHaveBeenCalled();
            })
        );
    });
});

class StubErrorHandlerService {
    handleUnexpectedError(error: any): void {
    }
}

class StubHttpService {
    get(url: string, options?: RequestOptionsArgs, domain?: string): Observable<Response> {
        return Observable.of(testResponse);
    };

    post(url: string, body: string, options?: RequestOptionsArgs, domain?: string): Observable<Response> {
        return Observable.of(testResponse);
    };
}
