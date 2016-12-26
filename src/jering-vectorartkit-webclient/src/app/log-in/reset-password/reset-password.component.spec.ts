import { Component, Input, Output, EventEmitter, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router/index';

import { ResetPasswordComponent } from './reset-password.component';
import { DynamicForm } from 'app/shared/dynamic-forms/dynamic-form/dynamic-form';
import { DynamicControl } from 'app/shared/dynamic-forms/dynamic-control/dynamic-control';
import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';

import { TwoFactorLogInResponseModel } from 'api/response-models/two-factor-log-in.response-model';

import { StubActivatedRoute } from 'testing/router-stubs';
import { StubDynamicFormComponent } from 'testing/dynamic-form.component.stub';
import { DebugElementExtensions } from 'testing/debug-element-extensions';

let testIsPersistent = true;
let testToken = `testToken`;
let testEmail = `testEmail`;
let resetPasswordComponentFixture: ComponentFixture<ResetPasswordComponent>;
let resetPasswordComponent: ResetPasswordComponent;
let resetPasswordDebugElement: DebugElement;
let testResetPasswordResponseModel: TwoFactorLogInResponseModel;
let stubActivatedRoute: StubActivatedRoute;
let ngAfterViewInitSpy: jasmine.Spy;

describe('ResetPasswordComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ResetPasswordComponent, StubDynamicFormComponent],
            providers: [{ provide: ActivatedRoute, useClass: StubActivatedRoute }]
        }).compileComponents();
    }));

    beforeEach(() => {
        resetPasswordComponentFixture = TestBed.createComponent(ResetPasswordComponent);
        resetPasswordComponent = resetPasswordComponentFixture.componentInstance;
        resetPasswordDebugElement = resetPasswordComponentFixture.debugElement;
        stubActivatedRoute = TestBed.get(ActivatedRoute) as StubActivatedRoute;
        stubActivatedRoute.testParams = { email: testEmail, token: testToken };
        // Prevent from being called since dynamicFormComponent needs to be setup manually
        ngAfterViewInitSpy = spyOn(resetPasswordComponent, `ngAfterViewInit`);
    });

    it(`ngOnInit sets email and token`, () => {
        resetPasswordComponentFixture.detectChanges();
        expect(resetPasswordComponent.email).toBe(testEmail);
        expect(resetPasswordComponent.token).toBe(testToken);
    });

    it(`ngAfterViewInit sets dynamicFormComponent's Email and Token DynamicControls values`, () => {
        resetPasswordComponentFixture.detectChanges();

        expect(resetPasswordComponent.ngAfterViewInit).toHaveBeenCalledTimes(1);

        let testDynamicForm = resetPasswordComponent.dynamicFormComponent.dynamicForm;
        let testEmailDynamicControl = new DynamicControl({});
        let testTokenDynamicControl = new DynamicControl({});
        spyOn(testDynamicForm, `getDynamicControl`).
            and.
            returnValues(testEmailDynamicControl, testTokenDynamicControl);
        ngAfterViewInitSpy.and.callThrough();

        resetPasswordComponent.ngAfterViewInit();

        expect(testDynamicForm.getDynamicControl).toHaveBeenCalledWith(`email`);
        expect(testDynamicForm.getDynamicControl).toHaveBeenCalledWith(`token`);
        expect(testEmailDynamicControl.value).toBe(testEmail);
        expect(testTokenDynamicControl.value).toBe(testToken);
    });

    it(`Listens to child DynamicFormComponent outputs`, () => {
        resetPasswordComponentFixture.detectChanges();
        spyOn(resetPasswordComponent, `onSubmitSuccess`);
        spyOn(resetPasswordComponent, `onSubmitError`);

        let anchorDebugElements = resetPasswordDebugElement.queryAll(By.css(`a`));

        anchorDebugElements.forEach(debugElement => debugElement.triggerEventHandler('click', null));

        expect(resetPasswordComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
        expect(resetPasswordComponent.onSubmitError).toHaveBeenCalledTimes(1);
    });

    it(`onSubmitSuccess sets passwordResetSuccessful to true`, () => {
        resetPasswordComponent.onSubmitSuccess(null);

        expect(resetPasswordComponent.passwordResetSuccessful).toBe(true);
    });

    describe(`onSubmitError`, () => {
        it(`Sets linkExpiredOrInvalid to true if responseModel.invalidEmail is true`, () => {
            let eventModel = new SubmitEventModel({ invalidEmail: true }, null);
            resetPasswordComponent.onSubmitError(eventModel);

            expect(resetPasswordComponent.linkExpiredOrInvalid).toBe(true);
        });
        it(`Sets linkExpiredOrInvalid to true if responseModel.invalidTokenis true`, () => {
            let eventModel = new SubmitEventModel({ invalidToken: true }, null);
            resetPasswordComponent.onSubmitError(eventModel);

            expect(resetPasswordComponent.linkExpiredOrInvalid).toBe(true);
        });
    });

    it(`Renders link expired tip and get new reset password link if linkExpiredOrInvalid is true`, () => {
        resetPasswordComponent.linkExpiredOrInvalid = true;

        resetPasswordComponentFixture.detectChanges();

        expect(DebugElementExtensions.hasDescendantWithInnerHtml(resetPasswordDebugElement, `Get new reset password link`)).toBe(true);
        expect(DebugElementExtensions.hasDescendantWithInnerHtml(resetPasswordDebugElement, `Link expired`)).toBe(true);
    });

    it(`Renders password reset tip and log in link if passwordResetSuccessful is true`, () => {
        resetPasswordComponent.passwordResetSuccessful = true;

        resetPasswordComponentFixture.detectChanges();

        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(resetPasswordDebugElement, `Password has been reset for account associated with email: ${testEmail}`)).toBe(true);
        expect(DebugElementExtensions.hasDescendantWithInnerHtml(resetPasswordDebugElement, `Log in`)).toBe(true);
    });

    it(`Renders reset password tip and dynamic form if passwordResetSuccessful and linkExpiredOrInvalid
        are both false`, () => {
            resetPasswordComponent.passwordResetSuccessful = false;
            resetPasswordComponent.linkExpiredOrInvalid = false;

            resetPasswordComponentFixture.detectChanges();

            expect(DebugElementExtensions.
                hasDescendantWithInnerHtml(resetPasswordDebugElement, `Reset password for account associated with email: ${testEmail}`)).toBe(true);
            expect(resetPasswordDebugElement.queryAll(By.css(`dynamic-form`)).length).toBe(1);
            expect(DebugElementExtensions.hasDescendantWithInnerHtml(resetPasswordDebugElement, `Get new reset password link`)).toBe(false);
            expect(DebugElementExtensions.hasDescendantWithInnerHtml(resetPasswordDebugElement, `Link expired`)).toBe(false);
            expect(DebugElementExtensions.
                hasDescendantWithInnerHtml(resetPasswordDebugElement, `Password has been reset for account associated with email: ${testEmail}`)).toBe(false);
            expect(DebugElementExtensions.hasDescendantWithInnerHtml(resetPasswordDebugElement, `Log in`)).toBe(false);
        });
});