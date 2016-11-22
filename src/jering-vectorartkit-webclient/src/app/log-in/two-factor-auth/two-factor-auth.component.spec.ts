import { Component, Input, Output, EventEmitter, DebugElement } from '@angular/core';
import { Response } from '@angular/http';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router, ActivatedRoute } from '@angular/router/index';

import { TwoFactorLogInResponseModel } from '../../shared/response-models/two-factor-log-in.response-model';
import { TwoFactorAuthComponent } from './two-factor-auth.component';
import { StubRouter, StubActivatedRoute } from '../../../testing/router-stubs';
import { UserService } from '../../shared/user.service';
import { DynamicForm } from '../../shared/dynamic-forms/dynamic-form/dynamic-form';

let testIsPersistent = true;
let testUsername = `testUsername`;
let testReturnUrl = `testReturnUrl`;
let twoFactorAuthComponentFixture: ComponentFixture<TwoFactorAuthComponent>;
let twoFactorAuthComponent: TwoFactorAuthComponent;
let twoFactorAuthDebugElement: DebugElement;
let testTwoFactorAuthResponseModel: TwoFactorLogInResponseModel;
let stubDynamicControl: StubDynamicControl;
let stubDynamicFormComponent: StubDynamicFormComponent;
let stubRouter: StubRouter;
let stubUserService: StubUserService;
let stubActivatedRoute: StubActivatedRoute;

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
        stubActivatedRoute.testParams = { returnUrl: testReturnUrl, isPersistent: testIsPersistent };
        stubDynamicFormComponent = twoFactorAuthComponent.dynamicFormComponent;
        stubDynamicControl = { value: null };
        spyOn(stubDynamicFormComponent.dynamicForm, `getDynamicControl`).and.returnValue(stubDynamicControl);
    });

    it(`Uses ViewChild to retrieve child DynamicFormsComponent and sets its IsPersistent DynamicControl's value`, () => {
        twoFactorAuthComponentFixture.detectChanges();
        expect(stubDynamicFormComponent).toBeDefined();
        expect(stubDynamicFormComponent.dynamicForm.getDynamicControl).toHaveBeenCalledWith(`IsPersistent`);
        expect(stubDynamicControl.value).toBe(testIsPersistent);
    });

    it(`Listens to child DynamicFormComponent output`, () => {
        spyOn(twoFactorAuthComponent, `onSubmitSuccess`);
        twoFactorAuthComponentFixture.detectChanges();
        let anchorDebugElement = twoFactorAuthDebugElement.query(By.css(`a`));

        anchorDebugElement.triggerEventHandler('click', null);

        expect(twoFactorAuthComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    describe(`onSubmitSuccess`, () => {
        beforeEach(() => {
            testTwoFactorAuthResponseModel = { username: testUsername, isPersistent: testIsPersistent }
        });

        it(`Calls UseService.login`, () => {
            spyOn(stubUserService, `logIn`);

            twoFactorAuthComponent.onSubmitSuccess(testTwoFactorAuthResponseModel);

            expect(stubUserService.logIn).toHaveBeenCalledWith(testUsername, testIsPersistent);
        });

        it(`Navigates to home if ActivatedRoute.snapshot.params.returnUrl is null`, () => {
            stubActivatedRoute.testParams = { returnUrl: null, isPersistent: testIsPersistent };
            spyOn(stubRouter, `navigate`);
            spyOn(stubUserService, `logIn`);

            twoFactorAuthComponent.onSubmitSuccess(testTwoFactorAuthResponseModel);

            expect(stubRouter.navigate).toHaveBeenCalledWith([`/home`]);
        });

        it(`Navigates to return url if ActivatedRoute.snapshot.params.returnUrl is defined`, () => {
            spyOn(stubRouter, `navigate`);
            spyOn(stubUserService, `logIn`);

            twoFactorAuthComponent.onSubmitSuccess(testTwoFactorAuthResponseModel);

            expect(stubRouter.navigate).toHaveBeenCalledWith([testReturnUrl]);
        });
    });
});

@Component({
    selector: `dynamic-form`,
    template: `<a (click)=submitSuccess.emit()></a>`
})
class StubDynamicFormComponent {
    @Output() submitSuccess = new EventEmitter<Response>();
    dynamicForm: DynamicForm = new DynamicForm([], null, null);
}

class StubUserService {
    logIn(username: string, isPersistent: boolean): void {
    }

    returnUrl: string;
}

class StubDynamicControl {
    value: string;
}