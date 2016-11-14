import { Component, Input, Output, EventEmitter, DebugElement } from '@angular/core';
import { Response } from '@angular/http';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router } from '@angular/router/index';

import { SignUpComponent } from './sign-up.component';
import { StubRouter } from '../../testing/router-stubs';

let signUpComponentFixture: ComponentFixture<SignUpComponent>;
let signUpComponent: SignUpComponent;
let signUpDebugElement: DebugElement;
let stubDynamicFormComponent: StubDynamicFormComponent;
let stubRouter: StubRouter;

describe('SignUpComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [SignUpComponent, StubDynamicFormComponent],
            providers: [{ provide: Router, useClass: StubRouter }]
        }).compileComponents();
    }));

    beforeEach(() => {
        signUpComponentFixture = TestBed.createComponent(SignUpComponent);
        signUpComponent = signUpComponentFixture.componentInstance;
        signUpDebugElement = signUpComponentFixture.debugElement;
        stubRouter = TestBed.get(Router) as StubRouter;

        //dynamicControl = stubHostComponent.dynamicControl;
        //dynamicControlComponent = stubHostComponent.dynamicControlComponent;
        //hostDebugElement = stubHostFixture.debugElement;
    });

    it(`Set up child DynamicFormComponent inputs`, () => {
        signUpComponentFixture.detectChanges();

        let divDebugElements = signUpDebugElement.queryAll(By.css(`div`));

        expect(divDebugElements.some(div => div.nativeElement.textContent === signUpComponent.formModelName)).toBe(true);
        expect(divDebugElements.some(div => div.nativeElement.textContent === signUpComponent.formSubmitUrl)).toBe(true);
    });

    it(`Listens to child DynamicFormComponent output`, () => {
        spyOn(signUpComponent, `onSubmitSuccess`);
        signUpComponentFixture.detectChanges();
        let anchorDebugElement = signUpDebugElement.query(By.css(`a`));

        anchorDebugElement.triggerEventHandler('click', null);

        expect(signUpComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    describe(`onSubmitSuccess`, () => {
        it(`Navigates to /home`, () => {
            spyOn(stubRouter, `navigate`);

            signUpComponent.onSubmitSuccess();

            expect(stubRouter.navigate).toHaveBeenCalledWith([`/home`]);
        });
    });
});

@Component({
    selector: `dynamic-form`,
    template: `<div>{{formModelName}}</div>
               <div>{{formSubmitUrl}}</div>
               <a (click)=submitSuccess.emit()></a>`
})
class StubDynamicFormComponent {
    @Input() formModelName: string;
    @Input() formSubmitUrl: string;
    @Output() submitSuccess = new EventEmitter<Response>();
}
