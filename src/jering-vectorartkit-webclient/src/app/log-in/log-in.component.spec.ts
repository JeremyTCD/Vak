import { Component, Input, Output, EventEmitter, DebugElement } from '@angular/core';
import { Response } from '@angular/http';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router } from '@angular/router/index';

import { LogInResponseModel } from '../shared/response-models/log-in.response-model';
import { LogInComponent } from './log-in.component';
import { StubRouter } from '../../testing/router-stubs';
import { UserService } from '../shared/user.service';

let testUsername = `testUsername`;
let testReturnUrl = `testReturnUrl`;
let logInComponentFixture: ComponentFixture<LogInComponent>;
let logInComponent: LogInComponent;
let logInDebugElement: DebugElement;
let testLogInResponseModel: LogInResponseModel;
let stubDynamicFormComponent: StubDynamicFormComponent;
let stubRouter: StubRouter;
let stubUserService: StubUserService;

describe('LogInComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [LogInComponent, StubDynamicFormComponent],
            providers: [{ provide: Router, useClass: StubRouter },
            { provide: UserService, useClass: StubUserService }]
        }).compileComponents();
    }));

    beforeEach(() => {
        logInComponentFixture = TestBed.createComponent(LogInComponent);
        logInComponent = logInComponentFixture.componentInstance;
        logInDebugElement = logInComponentFixture.debugElement;
        stubRouter = TestBed.get(Router) as StubRouter;
        stubUserService = TestBed.get(UserService) as StubUserService;
    });

    it(`Listens to child DynamicFormComponent output`, () => {
        spyOn(logInComponent, `onSubmitSuccess`);
        logInComponentFixture.detectChanges();
        let anchorDebugElement = logInDebugElement.query(By.css(`a`));

        anchorDebugElement.triggerEventHandler('click', null);

        expect(logInComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    describe(`onSubmitSuccess`, () => {
        it(`Navigates to /Login/twofactor if two factor authentication is required`, () => {
            testLogInResponseModel = { twoFactorRequired: true };
            spyOn(stubRouter, `navigate`);

            logInComponent.onSubmitSuccess(testLogInResponseModel);

            expect(stubRouter.navigate).toHaveBeenCalledWith([`/login/twofactor`]);
        });

        describe(`If two factor authentication is not required`, () => {
            it(`Calls UseService.login`, () => {
                testLogInResponseModel = { twoFactorRequired: false, username: testUsername, isPersistent: true };
                spyOn(stubUserService, `logIn`);

                logInComponent.onSubmitSuccess(testLogInResponseModel);

                expect(stubUserService.logIn).toHaveBeenCalledWith(testUsername, true);
            });

            it(`Navigates to home if UserService.returnUrl is null`, () => {
                testLogInResponseModel = { twoFactorRequired: false};
                stubUserService.returnUrl = null;
                spyOn(stubRouter, `navigate`);
                spyOn(stubUserService, `logIn`);

                logInComponent.onSubmitSuccess(testLogInResponseModel);

                expect(stubRouter.navigate).toHaveBeenCalledWith([`/home`]);
            });

            it(`Navigates to return url if UserService.returnUrl is defined`, () => {
                testLogInResponseModel = { twoFactorRequired: false };
                stubUserService.returnUrl = testReturnUrl;
                spyOn(stubRouter, `navigate`);
                spyOn(stubUserService, `logIn`);

                logInComponent.onSubmitSuccess(testLogInResponseModel);

                expect(stubRouter.navigate).toHaveBeenCalledWith([testReturnUrl]);
            });
        });
    });
});

@Component({
    selector: `dynamic-form`,
    template: `<a (click)=submitSuccess.emit()></a>`
})
class StubDynamicFormComponent {
    @Output() submitSuccess = new EventEmitter<Response>();
}

class StubUserService {
    logIn(username: string, isPersistent: boolean): void {
    }

    returnUrl: string;
}
