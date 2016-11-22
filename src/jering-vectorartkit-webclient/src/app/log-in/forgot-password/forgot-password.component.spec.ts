import { Component, Input, Output, EventEmitter, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';

import { ForgotPasswordComponent } from './forgot-password.component';

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
        forgotPasswordComponentFixture.detectChanges();
        let anchorDebugElement = forgotPasswordDebugElement.query(By.css(`a`));

        anchorDebugElement.triggerEventHandler('click', null);

        expect(forgotPasswordComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    it(`onSubmitSuccess sets submitSuccessful to true`, () => {
        forgotPasswordComponentFixture.detectChanges();
        forgotPasswordComponent.onSubmitSuccess(null);

        expect(forgotPasswordComponent.submitSuccessful).toBe(true);
    });

    it(`Renders reset password instructions and dynamic form if submitSuccessful is false`, () => {
        forgotPasswordComponentFixture.detectChanges();

        expect(forgotPasswordDebugElement.query(By.css(`dynamic-form`))).toBeTruthy();
        expect(forgotPasswordDebugElement.queryAll(By.css(`div`)).some(debugElement => debugElement.nativeElement.textContent.trim() === `Reset password instructions`)).toBe(true);
        expect(forgotPasswordDebugElement.queryAll(By.css(`div`)).some(debugElement => debugElement.nativeElement.textContent.trim() === `Reset password link sent`)).toBe(false);
        // 1 anchor from StubDynamicFormComponent
        expect(forgotPasswordDebugElement.queryAll(By.css(`a`)).length).toBe(2);
    });

    xit(`Renders reset password instructions and dynamic form if submitSuccessful is false`, () => {
        forgotPasswordComponentFixture.detectChanges();

        expect(forgotPasswordDebugElement.queryAll(By.css(`a`)).length).toBe(3);
        expect(forgotPasswordDebugElement.queryAll(By.css(`div`)).some(debugElement => debugElement.nativeElement.textContent.trim() === `Reset password link sent`)).toBe(true);
        expect(forgotPasswordDebugElement.query(By.css(`dynamic-form`))).toBe(null);
        expect(forgotPasswordDebugElement.queryAll(By.css(`div`)).some(debugElement => debugElement.nativeElement.textContent.trim() === `Reset password instructions`)).toBe(false);
    });
});

@Component({
    selector: `dynamic-form`,
    template: `<a (click)=submitSuccess.emit()></a>`
})
class StubDynamicFormComponent {
    @Output() submitSuccess = new EventEmitter<any>();
}