import { TestBed, inject } from '@angular/core/testing';
import { Response, ResponseOptions, RequestOptionsArgs, URLSearchParams } from '@angular/http';
import { Observable } from 'rxjs/Observable';

import { DynamicControl } from './dynamic-control/dynamic-control';
import { DynamicForm } from './dynamic-form/dynamic-form';
import { DynamicFormResponseModel } from './response-models/dynamic-form.response-model';
import { DynamicFormsService } from './dynamic-forms.service';
import { ValidateResponseModel } from '../response-models/validate.response-model';
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
let testDynamicFormResponseModel: DynamicFormResponseModel;

describe('DynamicFormsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [DynamicFormsService, { provide: HttpService, useClass: StubHttpService }]
        });
        testDynamicControl = new DynamicControl<any>({ name: testControlName });
        testDynamicForm = new DynamicForm([testDynamicControl], testMessage);
        testResponse = new Response(
            new ResponseOptions({})
        );
    });

    describe(`getDynamicForm`, () => {
        beforeEach(() => {
            testDynamicFormResponseModel = { dynamicControlResponseModels: [], errorMessage: testMessage};
        });

        it('Calls Http.get',
            inject([DynamicFormsService, HttpService], (dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                let getSpy = spyOn(stubHttpService, `get`).and.returnValue(Observable.of(testDynamicFormResponseModel));

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

        it(`Maps response to a DynamicFormResponseModel if Http.get succeeds`,
            inject([DynamicFormsService, HttpService], (dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                spyOn(stubHttpService, `get`).and.returnValue(Observable.of(testDynamicFormResponseModel));
                let result: DynamicForm;

                dynamicFormsService.
                    getDynamicForm(testFormModelName).
                    subscribe(dynamicForm => result = dynamicForm);

                expect(result instanceof DynamicForm).toBe(true);
                expect(result.dynamicControls.length).toBe(0);
                expect(result.message).toBe(testMessage);
            })
        );
    });

    it(`submitDynamicForm calls Http.post`, inject([DynamicFormsService, HttpService], (dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
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

        it(`Maps ValidateResponseModel to a Validity value if Http.get succeeds`,
            inject([DynamicFormsService, HttpService], (dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                let testValidateResponseModel: ValidateResponseModel = {valid: true};
                testResponse = new Response(
                    new ResponseOptions({
                        body: JSON.stringify(testValidateResponseModel)
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
    });
});

class StubHttpService {
    get(url: string, options?: RequestOptionsArgs, domain?: string): Observable<any> {
        return Observable.of(testResponse);
    };

    post(url: string, body: string, options?: RequestOptionsArgs, domain?: string): Observable<any> {
        return Observable.of(testResponse);
    };
}
