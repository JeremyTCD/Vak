import { Component, Input, Output, EventEmitter, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';

import { ForgotPasswordComponent } from './forgot-password.component';

import { StubDynamicFormComponent } from 'testing/dynamic-form.component.stub';
import { DebugElementExtensions } from 'testing/debug-element-extensions';

let forgotPasswordComponentFixture: ComponentFixture<ForgotPasswordComponent>;
let forgotPasswordComponent: ForgotPasswordComponent;
let forgotPasswordDebugElement: DebugElement;

describe('ForgotPasswordComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ForgotPasswordComponent, StubDynamicFormComponent]
        }).compileComponents();
    }));

    beforeEach(() => {
        forgotPasswordComponentFixture = TestBed.createComponent(ForgotPasswordComponent);
        forgotPasswordComponent = forgotPasswordComponentFixture.componentInstance;
        forgotPasswordDebugElement = forgotPasswordComponentFixture.debugElement;
    });

    it(`Listens to child DynamicFormComponent output`, () => {
        spyOn(forgotPasswordComponent, `onSubmitSuccess`);
        spyOn(forgotPasswordComponent, `onSubmitError`);

        forgotPasswordComponentFixture.detectChanges();
        let anchorDebugElement = forgotPasswordDebugElement.query(By.css(`a`));

        anchorDebugElement.triggerEventHandler('click', null);

        expect(forgotPasswordComponent.onSubmitError).toHaveBeenCalledTimes(1);
        expect(forgotPasswordComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    it(`onSubmitSuccess sets submitSuccessful to true`, () => {
        forgotPasswordComponent.onSubmitSuccess(null);

        expect(forgotPasswordComponent.submitSuccessful).toBe(true);
    });

    it(`onSubmitError sets submitSuccessful to true`, () => {
        forgotPasswordComponent.onSubmitError(null);

        expect(forgotPasswordComponent.submitSuccessful).toBe(true);
    });

    it(`Renders reset password instructions and dynamic form if submitSuccessful is false`, () => {
        forgotPasswordComponent.submitSuccessful = false;

        forgotPasswordComponentFixture.detectChanges();

        expect(forgotPasswordDebugElement.
            queryAll(By.css(`dynamic-form`)).length).toBe(1);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(forgotPasswordDebugElement, `Reset password instructions`)).toBe(true);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(forgotPasswordDebugElement, `Reset password link sent`)).toBe(false);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(forgotPasswordDebugElement, `Did not receive email help`)).toBe(false);
    });

    it(`Renders reset password link sent tip and did not recieve email tip if submitSuccessful is true`, () => {
        forgotPasswordComponent.submitSuccessful = true;

        forgotPasswordComponentFixture.detectChanges();

        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(forgotPasswordDebugElement, `Reset password instructions`)).toBe(false);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(forgotPasswordDebugElement, `Reset password link sent`)).toBe(true);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(forgotPasswordDebugElement, `Did not receive email help`)).toBe(true);
    });
});