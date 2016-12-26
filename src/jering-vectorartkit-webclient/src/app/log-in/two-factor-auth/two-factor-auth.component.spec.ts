import { Component, Input, Output, EventEmitter, DebugElement } from '@angular/core';
import { Response } from '@angular/http';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router, ActivatedRoute } from '@angular/router/index';

import { TwoFactorAuthComponent } from './two-factor-auth.component';
import { UserService } from 'app/shared/user.service';
import { DynamicForm } from 'app/shared/dynamic-forms/dynamic-form/dynamic-form';
import { DynamicControl } from 'app/shared/dynamic-forms/dynamic-control/dynamic-control';
import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';
import { AppPaths } from 'app/app.paths';

import { TwoFactorLogInResponseModel } from 'api/response-models/two-factor-log-in.response-model';

import { StubUserService } from 'testing/user.service.stub';
import { StubRouter, StubActivatedRoute } from 'testing/router-stubs';
import { StubDynamicFormComponent } from 'testing/dynamic-form.component.stub';
import { DebugElementExtensions } from 'testing/debug-element-extensions';

let testIsPersistent = true;
let testUsername = `testUsername`;
let testReturnUrl = `testReturnUrl`;
let twoFactorAuthComponentFixture: ComponentFixture<TwoFactorAuthComponent>;
let twoFactorAuthComponent: TwoFactorAuthComponent;
let twoFactorAuthDebugElement: DebugElement;
let testTwoFactorAuthResponseModel: TwoFactorLogInResponseModel;
let stubDynamicFormComponent: StubDynamicFormComponent;
let stubRouter: StubRouter;
let stubUserService: StubUserService;
let stubActivatedRoute: StubActivatedRoute;
let ngAfterViewInitSpy: jasmine.Spy;


describe('TwoFactorAuthComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [TwoFactorAuthComponent, StubDynamicFormComponent],
            providers: [{ provide: Router, useClass: StubRouter },
            { provide: UserService, useClass: StubUserService },
            { provide: ActivatedRoute, useClass: StubActivatedRoute }]
        }).compileComponents();
    }));

    beforeEach(() => {
        twoFactorAuthComponentFixture = TestBed.createComponent(TwoFactorAuthComponent);
        twoFactorAuthComponent = twoFactorAuthComponentFixture.componentInstance;
        twoFactorAuthDebugElement = twoFactorAuthComponentFixture.debugElement;
        stubRouter = TestBed.get(Router) as StubRouter;
        stubUserService = TestBed.get(UserService) as StubUserService;
        stubActivatedRoute = TestBed.get(ActivatedRoute) as StubActivatedRoute;
        stubActivatedRoute.testParams = {
            username: testUsername,
            returnUrl: testReturnUrl,
            isPersistent: testIsPersistent.toString()
        };
        // Prevent from being called since dynamicFormComponent needs to be setup manually
        ngAfterViewInitSpy = spyOn(twoFactorAuthComponent, `ngAfterViewInit`);
    });

    it(`ngOnInit sets isPersistent, username and returnUrl`, () => {
        twoFactorAuthComponentFixture.detectChanges();
        expect(twoFactorAuthComponent.isPersistent).toBe(testIsPersistent);
        expect(twoFactorAuthComponent.username).toBe(testUsername);
        expect(twoFactorAuthComponent.returnUrl).toBe(testReturnUrl);
    });

    it(`ngAfterViewInit sets dynamicFormComponent's IsPersistent DynamicControl's value`, () => {
        twoFactorAuthComponentFixture.detectChanges();

        expect(twoFactorAuthComponent.ngAfterViewInit).toHaveBeenCalledTimes(1);

        let testDynamicForm = twoFactorAuthComponent.dynamicFormComponent.dynamicForm;
        let testIsPersistentDynamicControl = new DynamicControl({});
        spyOn(testDynamicForm, `getDynamicControl`).
            and.
            returnValue(testIsPersistentDynamicControl);
        ngAfterViewInitSpy.and.callThrough();

        twoFactorAuthComponent.ngAfterViewInit();

        expect(testDynamicForm.getDynamicControl).toHaveBeenCalledWith(`isPersistent`);
        expect(testIsPersistentDynamicControl.value).toBe(testIsPersistent.toString());
    });

    it(`Listens to child DynamicFormComponent output`, () => {
        twoFactorAuthComponentFixture.detectChanges();
        spyOn(twoFactorAuthComponent, `onSubmitSuccess`);
        spyOn(twoFactorAuthComponent, `onSubmitError`);
        let anchorDebugElement = twoFactorAuthDebugElement.query(By.css(`a`));

        anchorDebugElement.triggerEventHandler('click', null);

        expect(twoFactorAuthComponent.onSubmitError).toHaveBeenCalledTimes(1);
        expect(twoFactorAuthComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    describe(`onSubmitSuccess`, () => {
        it(`Calls UseService.login`, () => {
            twoFactorAuthComponent.username = testUsername;
            twoFactorAuthComponent.isPersistent = testIsPersistent;
            spyOn(stubUserService, `logIn`);

            twoFactorAuthComponent.onSubmitSuccess(null);

            expect(stubUserService.logIn).toHaveBeenCalledWith(testUsername, testIsPersistent);
        });

        it(`Navigates to home if ActivatedRoute.snapshot.params.returnUrl is null`, () => {
            twoFactorAuthComponent.returnUrl = null;
            spyOn(stubRouter, `navigate`);
            spyOn(stubUserService, `logIn`);

            twoFactorAuthComponent.onSubmitSuccess(null);

            expect(stubRouter.navigate).toHaveBeenCalledWith([AppPaths.homePath]);
        });

        it(`Navigates to return url if ActivatedRoute.snapshot.params.returnUrl is defined`, () => {
            twoFactorAuthComponent.returnUrl = testReturnUrl;
            spyOn(stubRouter, `navigate`);
            spyOn(stubUserService, `logIn`);

            twoFactorAuthComponent.onSubmitSuccess(null);

            expect(stubRouter.navigate).toHaveBeenCalledWith([testReturnUrl]);
        });
    });

    it(`onSubmitError sets codeExpired to true if twoFactorAuthResponseModel.expiredCredentials is true`, () => {
        testTwoFactorAuthResponseModel = { expiredCredentials: true };
        let eventModel = new SubmitEventModel(testTwoFactorAuthResponseModel, null);

        twoFactorAuthComponent.onSubmitError(eventModel);

        expect(twoFactorAuthComponent.codeExpired).toBe(true);
    });

    it(`Renders DynamicForm if codeExpired is false`, () => {
        twoFactorAuthComponent.codeExpired = false;
        twoFactorAuthComponentFixture.detectChanges();

        expect(twoFactorAuthDebugElement.queryAll(By.css(`dynamic-form`)).length).toBe(1);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(twoFactorAuthDebugElement, `Code expired. Log in again to obtain a new code.`)).toBe(false);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(twoFactorAuthDebugElement, `Log in`)).toBe(false);
    });

    it(`Renders code expired tip and log in link if codeExpired is true`, () => {
        twoFactorAuthComponent.codeExpired = true;
        twoFactorAuthComponentFixture.detectChanges();

        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(twoFactorAuthDebugElement, `Code expired. Log in again to obtain a new code.`)).toBe(true);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(twoFactorAuthDebugElement, `Log in`)).toBe(true);
        expect(twoFactorAuthDebugElement.queryAll(By.css(`dynamic-form`)).length).toBe(0);
    });
});