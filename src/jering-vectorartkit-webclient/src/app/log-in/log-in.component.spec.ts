import { Component, Input, Output, EventEmitter, DebugElement } from '@angular/core';
import { Response } from '@angular/http';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router, ActivatedRoute } from '@angular/router/index';

import { LogInResponseModel } from '../shared/response-models/log-in.response-model';
import { LogInComponent } from './log-in.component';
import { StubRouter, StubActivatedRoute } from '../../testing/router-stubs';
import { UserService } from '../shared/user.service';

let testUsername = `testUsername`;
let testReturnUrl = `testReturnUrl`;
let logInComponentFixture: ComponentFixture<LogInComponent>;
let logInComponent: LogInComponent;
let logInDebugElement: DebugElement;
let testLogInResponseModel: LogInResponseModel;
let stubRouter: StubRouter;
let stubUserService: StubUserService;
let stubActivatedRoute: StubActivatedRoute;

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

    describe(`onSubmitSuccess`, () => {
        it(`Calls UseService.login`, () => {
            stubActivatedRoute.testParams = { returnUrl: testReturnUrl };
            testLogInResponseModel = { twoFactorRequired: false, username: testUsername, isPersistent: true };
            spyOn(stubUserService, `logIn`);

            logInComponent.onSubmitSuccess(testLogInResponseModel);

            expect(stubUserService.logIn).toHaveBeenCalledWith(testUsername, true);
        });

        it(`Navigates to home if ActivatedRoute.snapshot.params.returnUrl is null`, () => {
            stubActivatedRoute.testParams = { returnUrl: null };
            testLogInResponseModel = { twoFactorRequired: false};
            spyOn(stubRouter, `navigate`);
            spyOn(stubUserService, `logIn`);

            logInComponent.onSubmitSuccess(testLogInResponseModel);

            expect(stubRouter.navigate).toHaveBeenCalledWith([`/home`]);
        });

        it(`Navigates to return url if ActivatedRoute.snapshot.params.returnUrl is defined`, () => {
            stubActivatedRoute.testParams = { returnUrl: testReturnUrl };
            testLogInResponseModel = { twoFactorRequired: false };
            spyOn(stubRouter, `navigate`);
            spyOn(stubUserService, `logIn`);

            logInComponent.onSubmitSuccess(testLogInResponseModel);

            expect(stubRouter.navigate).toHaveBeenCalledWith([testReturnUrl]);
        });
    });

    describe(`onSubmitError`, () => {
        it(`Navigates to /Login/TwoFactorAuth if two factor auth is required`, () => {
            stubActivatedRoute.testParams = { returnUrl: testReturnUrl };
            testLogInResponseModel = { twoFactorRequired: true, isPersistent: false };
            spyOn(stubRouter, `navigate`);

            logInComponent.onSubmitError(testLogInResponseModel);

            expect(stubRouter.navigate).toHaveBeenCalledWith([`/login/two-factor-auth`, { isPersistent: false, returnUrl: testReturnUrl }]);
        });
    });
});

@Component({
    selector: `dynamic-form`,
    template: `<a (click)="submitSuccess.emit();submitError.emit();"></a>`
})
class StubDynamicFormComponent {
    @Output() submitSuccess = new EventEmitter<Response>();
    @Output() submitError = new EventEmitter<Response>();
}

class StubUserService {
    logIn(username: string, isPersistent: boolean): void {
    }

    returnUrl: string;
}
