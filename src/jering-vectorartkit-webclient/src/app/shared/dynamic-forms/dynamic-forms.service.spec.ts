import { TestBed, inject } from '@angular/core/testing';
import { Response, ResponseOptions, RequestOptionsArgs, URLSearchParams } from '@angular/http';
import { Observable } from 'rxjs/Observable';

import { DynamicControl } from './dynamic-control/dynamic-control';
import { DynamicForm } from './dynamic-form/dynamic-form';
import { DynamicFormResponseModel } from 'api/response-models/dynamic-form.response-model';
import { DynamicFormsService } from './dynamic-forms.service';
import { ValidateResponseModel } from 'api/response-models/validate.response-model';
import { environment } from 'environments/environment';
import { Validity } from './validity';
import { HttpService } from '../http.service';
import { StubHttpService } from 'testing/http.service.stub';

let testRequestModelName = `testRequestModelName`;
let testControlName = `testControlName`;
let testMessage = `testMessage`;
let testButtonText = `testButtonText`;
let testRelativeUrl = `testRelativeUrl`;
let testDynamicControl: DynamicControl;
let testDynamicForm: DynamicForm;
let testResponse: Response;
let testDynamicFormResponseModel: DynamicFormResponseModel;

describe('DynamicFormsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [DynamicFormsService, { provide: HttpService, useClass: StubHttpService }]
        });
        testDynamicControl = new DynamicControl({ name: testControlName });
        testDynamicForm = new DynamicForm([testDynamicControl], testMessage, testButtonText);
        testResponse = new Response(
            new ResponseOptions({})
        );
    });

    describe(`getDynamicForm`, () => {
        beforeEach(() => {
            testDynamicFormResponseModel = { dynamicControlResponseModels: [], errorMessage: testMessage, buttonText: testButtonText};
        });

        it('Calls Http.get',
            inject([DynamicFormsService, HttpService], (dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                let getSpy = spyOn(stubHttpService, `get`).and.returnValue(Observable.of(testDynamicFormResponseModel));

                dynamicFormsService.
                    getDynamicForm(testRequestModelName, true).
                    subscribe(dynamicForm => null);

                expect(stubHttpService.get).toHaveBeenCalledTimes(1);
                let args = getSpy.calls.first().args;
                expect(args[0]).toBe(`DynamicForm/GetDynamicFormWithAfTokens`);
                let requestOptions = args[1] as RequestOptionsArgs;
                let urlSearchParams = requestOptions.search as URLSearchParams;
                expect(urlSearchParams.get(`requestModelName`)).toBe(testRequestModelName);
            })
        );

        it(`Maps response to a DynamicFormResponseModel if Http.get succeeds`,
            inject([DynamicFormsService, HttpService], (dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
                spyOn(stubHttpService, `get`).and.returnValue(Observable.of(testDynamicFormResponseModel));
                let result: DynamicForm;

                dynamicFormsService.
                    getDynamicForm(testRequestModelName, true).
                    subscribe(dynamicForm => result = dynamicForm);

                expect(result instanceof DynamicForm).toBe(true);
                expect(result.dynamicControls.length).toBe(0);
                expect(result.message).toBe(testMessage);
                expect(result.buttonText).toBe(testButtonText);
            })
        );
    });

    it(`submitDynamicForm calls HttpService.post`, inject([DynamicFormsService, HttpService], (dynamicFormsService: DynamicFormsService, stubHttpService: StubHttpService) => {
        let getSpy = spyOn(stubHttpService, `post`).and.returnValue(Observable.of(testResponse));

        dynamicFormsService.
            submitDynamicForm(testRelativeUrl, testDynamicForm.value).
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


