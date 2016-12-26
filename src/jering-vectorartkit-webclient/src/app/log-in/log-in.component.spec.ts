import { Component, Input, Output, EventEmitter, DebugElement } from '@angular/core';
import { Response } from '@angular/http';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router, ActivatedRoute } from '@angular/router/index';

import { LogInComponent } from './log-in.component';
import { UserService } from 'app/shared/user.service';
import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';
import { AppPaths } from 'app/app.paths';

import { LogInResponseModel } from 'api/response-models/log-in.response-model';
import { LogInRequestModel } from 'api/request-models/log-in.request-model';

import { StubUserService } from 'testing/user.service.stub';
import { StubDynamicFormComponent } from 'testing/dynamic-form.component.stub';
import { StubRouter, StubActivatedRoute } from 'testing/router-stubs';

let testUsername = `testUsername`;
let testReturnUrl = `testReturnUrl`;
let logInComponentFixture: ComponentFixture<LogInComponent>;
let logInComponent: LogInComponent;
let logInDebugElement: DebugElement;
let testLogInResponseModel: LogInResponseModel;
let stubRouter: StubRouter;
let stubUserService: StubUserService;
let stubActivatedRoute: StubActivatedRoute;
let testSubmitEventModel: SubmitEventModel;

describe('LogInComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [LogInComponent, StubDynamicFormComponent],
            providers: [{ provide: Router, useClass: StubRouter },
                { provide: UserService, useClass: StubUserService },
                { provide: ActivatedRoute, useClass: StubActivatedRoute }]
        }).compileComponents();
    }));

    beforeEach(() => {
        logInComponentFixture = TestBed.createComponent(LogInComponent);
        logInComponent = logInComponentFixture.componentInstance;
        logInDebugElement = logInComponentFixture.debugElement;
        stubRouter = TestBed.get(Router) as StubRouter;
        stubUserService = TestBed.get(UserService) as StubUserService;
        stubActivatedRoute = TestBed.get(ActivatedRoute) as StubActivatedRoute;
        stubActivatedRoute.testParams = { returnUrl: testReturnUrl };
    });

    it(`Listens to child DynamicFormComponent output`, () => {
        spyOn(logInComponent, `onSubmitSuccess`);
        spyOn(logInComponent, `onSubmitError`);
        logInComponentFixture.detectChanges();
        let anchorDebugElement = logInDebugElement.query(By.css(`a`));

        anchorDebugElement.triggerEventHandler('click', null);

        expect(logInComponent.onSubmitError).toHaveBeenCalledTimes(1);
        expect(logInComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    it(`ngOnInit sets returnUrl`, () => {
        logInComponentFixture.detectChanges();

        expect(logInComponent.returnUrl).toBe(testReturnUrl);
    });

    describe(`onSubmitSuccess`, () => {
        it(`Calls UseService.login`, () => {
            let requestModel: LogInRequestModel = { email: testUsername, rememberMe: `true` };
            testSubmitEventModel = new SubmitEventModel(null, requestModel);
            spyOn(stubUserService, `logIn`);

            logInComponent.onSubmitSuccess(testSubmitEventModel);

            expect(stubUserService.logIn).toHaveBeenCalledWith(testUsername, true);
        });
           
        it(`Navigates to home if ActivatedRoute.snapshot.params.returnUrl is null`, () => {
            logInComponent.returnUrl = null;
            spyOn(stubRouter, `navigate`);
            spyOn(stubUserService, `logIn`);

            logInComponent.onSubmitSuccess(new SubmitEventModel(null, {}));

            expect(stubRouter.navigate).toHaveBeenCalledWith([AppPaths.homePath]);
        });

        it(`Navigates to return url if ActivatedRoute.snapshot.params.returnUrl is defined`, () => {
            logInComponent.returnUrl = testReturnUrl;
            spyOn(stubRouter, `navigate`);
            spyOn(stubUserService, `logIn`);

            logInComponent.onSubmitSuccess(new SubmitEventModel(null, {}));

            expect(stubRouter.navigate).toHaveBeenCalledWith([testReturnUrl]);
        });
    });

    describe(`onSubmitError`, () => {
        it(`Navigates to /Login/TwoFactorAuth if two factor auth is required`, () => {
            logInComponent.returnUrl = testReturnUrl;
            let responseModel: LogInResponseModel = { twoFactorRequired: true };
            let requestModel: LogInRequestModel = { email: testUsername, rememberMe: `true` };
            testSubmitEventModel = new SubmitEventModel(responseModel, requestModel );
            spyOn(stubRouter, `navigate`);

            logInComponent.onSubmitError(testSubmitEventModel);

            expect(stubRouter.navigate).toHaveBeenCalledWith([AppPaths.twoFactorAuthPath,
                { username: testUsername, isPersistent: `true`, returnUrl: testReturnUrl }]);
        });
    });
});