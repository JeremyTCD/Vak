import { Component, Input, Output, EventEmitter, DebugElement } from '@angular/core';
import { Response } from '@angular/http';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router } from '@angular/router/index';

import { SignUpComponent } from './sign-up.component';
import { StubRouter } from '../../testing/router-stubs';
import { UserService } from '../shared/user.service';

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

    it(`Set up child DynamicFormComponent input`, () => {
        signUpComponentFixture.detectChanges();

        let divDebugElements = signUpDebugElement.queryAll(By.css(`div`));

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
        it(`Navigates to /home and calls UserService.logIn`, () => {
            spyOn(stubRouter, `navigate`);
            spyOn(stubUserService, `logIn`);

            signUpComponent.onSubmitSuccess({username: testUsername});

            expect(stubRouter.navigate).toHaveBeenCalledWith([`/home`]);
            expect(stubUserService.logIn).toHaveBeenCalledWith(testUsername, false);
        });
    });
});

@Component({
    selector: `dynamic-form`,
    template: `<div>{{formSubmitUrl}}</div>
               <a (click)=submitSuccess.emit()></a>`
})
class StubDynamicFormComponent {
    @Input() formSubmitUrl: string;
    @Output() submitSuccess = new EventEmitter<Response>();
}

class StubUserService {
    logIn(username: string, isPersistent: boolean): void {
    }
}
