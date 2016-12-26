import { Component, Input, Output, EventEmitter, DebugElement } from '@angular/core';
import { Response } from '@angular/http';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router } from '@angular/router/index';

import { SignUpComponent } from './sign-up.component';
import { AppPaths } from 'app/app.paths';
import { UserService } from 'app/shared/user.service';
import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';

import { SignUpRequestModel } from 'api/request-models/sign-up.request-model';

import { StubRouter } from 'testing/router-stubs';
import { StubDynamicFormComponent } from 'testing/dynamic-form.component.stub';
import { StubUserService } from 'testing/user.service.stub';

let testUsername = `testUsername`;
let signUpComponentFixture: ComponentFixture<SignUpComponent>;
let signUpComponent: SignUpComponent;
let signUpDebugElement: DebugElement;
let stubDynamicFormComponent: StubDynamicFormComponent;
let stubRouter: StubRouter;
let stubUserService: StubUserService;

describe('SignUpComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [SignUpComponent, StubDynamicFormComponent],
            providers: [{ provide: Router, useClass: StubRouter },
                { provide: UserService, useClass: StubUserService }]
        }).compileComponents();
    }));

    beforeEach(() => {
        signUpComponentFixture = TestBed.createComponent(SignUpComponent);
        signUpComponent = signUpComponentFixture.componentInstance;
        signUpDebugElement = signUpComponentFixture.debugElement;
        stubRouter = TestBed.get(Router) as StubRouter;
        stubUserService = TestBed.get(UserService) as StubUserService;
    });

    it(`Listens to child DynamicFormComponent output`, () => {
        spyOn(signUpComponent, `onSubmitSuccess`);
        signUpComponentFixture.detectChanges();
        let anchorDebugElement = signUpDebugElement.query(By.css(`a`));

        anchorDebugElement.triggerEventHandler('click', null);

        expect(signUpComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    describe(`onSubmitSuccess`, () => {
        it(`Navigates to /home and calls UserService.logIn`, () => {
            spyOn(stubRouter, `navigate`);
            spyOn(stubUserService, `logIn`);
            let requestModel: SignUpRequestModel = { email: testUsername };
            let eventModel: SubmitEventModel = new SubmitEventModel(null, requestModel);

            signUpComponent.onSubmitSuccess(eventModel);

            expect(stubRouter.navigate).toHaveBeenCalledWith([AppPaths.homePath]);
            expect(stubUserService.logIn).toHaveBeenCalledWith(testUsername, true);
        });
    });
});
