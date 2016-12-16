import { Component, Input, Output, EventEmitter, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router/index';

import { TwoFactorLogInResponseModel } from '../../shared/response-models/two-factor-log-in.response-model';
import { ResetPasswordComponent } from './reset-password.component';
import { StubActivatedRoute } from '../../../testing/router-stubs';
import { DynamicForm } from '../../shared/dynamic-forms/dynamic-form/dynamic-form';
import { DynamicControl } from '../../shared/dynamic-forms/dynamic-control/dynamic-control';

let testIsPersistent = true;
let testToken = `testToken`;
let testEmail = `testEmail`;
let resetPasswordComponentFixture: ComponentFixture<ResetPasswordComponent>;
let resetPasswordComponent: ResetPasswordComponent;
let resetPasswordDebugElement: DebugElement;
let testResetPasswordResponseModel: TwoFactorLogInResponseModel;
let testEmailDynamicControl: DynamicControl;
let testTokenDynamicControl: DynamicControl;
let stubDynamicFormComponent: StubDynamicFormComponent;
let stubActivatedRoute: StubActivatedRoute;

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
        testEmailDynamicControl = new DynamicControl({ name: `Email` });
        testTokenDynamicControl = new DynamicControl({ name: `Token` });

        // dynamicFormComponent only rendered after initial data binding
        resetPasswordComponentFixture.detectChanges();
        stubDynamicFormComponent = resetPasswordComponent.dynamicFormComponent;
    });

    it(`ngOnInit sets email and token`, () => {
        resetPasswordComponentFixture.detectChanges();
        expect(resetPasswordComponent.email).toBe(testEmail);
        expect(resetPasswordComponent.token).toBe(testToken);
    });

    it(`Uses ViewChild to retrieve child DynamicFormsComponent and sets its Email and Token DynamicControls values`, () => {
        expect(stubDynamicFormComponent).toBeDefined();
        expect(stubDynamicFormComponent.dynamicForm.getDynamicControl(`Email`).value).toBe(testEmail);
        expect(stubDynamicFormComponent.dynamicForm.getDynamicControl(`Token`).value).toBe(testToken);
    });

    it(`Listens to child DynamicFormComponent outputs`, () => {
        spyOn(resetPasswordComponent, `onSubmitSuccess`);
        spyOn(resetPasswordComponent, `onSubmitError`);

        let anchorDebugElements = resetPasswordDebugElement.queryAll(By.css(`a`));

        anchorDebugElements.forEach(debugElement => debugElement.triggerEventHandler('click', null));

        expect(resetPasswordComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
        expect(resetPasswordComponent.onSubmitError).toHaveBeenCalledTimes(1);
    });

    it(`onSubmitSuccess sets passwordResetSuccessful to true`, () => {
        resetPasswordComponentFixture.detectChanges();
        resetPasswordComponent.onSubmitSuccess(null);

        expect(resetPasswordComponent.passwordResetSuccessful).toBe(true);
    });

    it(`onSubmitError sets linkExpiredOrInvalid to true if responseModel.invalidToken or responseModel.invalidEmail is true`, () => {
        resetPasswordComponentFixture.detectChanges();
        resetPasswordComponent.onSubmitError({ invalidToken: true, invalidEmail: true });

        expect(resetPasswordComponent.linkExpiredOrInvalid).toBe(true);
    });

    it(`Renders link expired tip and get new reset password link if linkExpiredOrInvalid is true`, () => {
        resetPasswordComponent.linkExpiredOrInvalid = true;

        resetPasswordComponentFixture.detectChanges();

        expect(resetPasswordDebugElement.queryAll(By.css(`a`)).length).toBe(1);
        expect(resetPasswordDebugElement.query(By.css(`a`)).nativeElement.textContent.trim()).toBe(`Get new reset password link`);
        let divs = resetPasswordDebugElement.queryAll(By.css(`div`));
        expect(divs.length).toBe(3);
        expect(divs.some(debugElement => debugElement.nativeElement.textContent.trim() === `Link expired`)).toBe(true);
    });

    it(`Renders password reset tip and log in link if passwordResetSuccessful is true`, () => {
        resetPasswordComponent.passwordResetSuccessful = true;

        resetPasswordComponentFixture.detectChanges();

        expect(resetPasswordDebugElement.queryAll(By.css(`a`)).length).toBe(1);
        expect(resetPasswordDebugElement.query(By.css(`a`)).nativeElement.textContent.trim()).toBe(`Log in`);
        let divs = resetPasswordDebugElement.queryAll(By.css(`div`));
        expect(divs.length).toBe(3);
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Password has been reset for account associated with email: ${testEmail}`)).toBe(true);
    });

    it(`Renders reset password tip and dynamic form if passwordResetSuccessful and linkExpiredOrInvalid
        are both false`, () => {
            resetPasswordComponent.passwordResetSuccessful = false;
            resetPasswordComponent.linkExpiredOrInvalid = false;

            resetPasswordComponentFixture.detectChanges();

            expect(resetPasswordDebugElement.queryAll(By.css(`dynamic-form`)).length).toBe(1);
            let divs = resetPasswordDebugElement.queryAll(By.css(`div`));
            expect(divs.length).toBe(3);
            expect(divs.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Reset password for account associated with email: ${testEmail}`)).toBe(true);
        });
});

@Component({
    selector: `dynamic-form`,
    template: `<a (click)=submitSuccess.emit()></a><a (click)=submitError.emit()></a>`
})
class StubDynamicFormComponent {
    @Output() submitSuccess = new EventEmitter<any>();
    @Output() submitError = new EventEmitter<any>();
    dynamicForm: DynamicForm = new DynamicForm([testEmailDynamicControl, testTokenDynamicControl], null, null);
}

class StubUserService {
    logIn(username: string, isPersistent: boolean): void {
    }

    returnUrl: string;
}
