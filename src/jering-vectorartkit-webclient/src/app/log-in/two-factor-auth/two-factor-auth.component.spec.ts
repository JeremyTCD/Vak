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
import { DynamicControl } from '../../shared/dynamic-forms/dynamic-control/dynamic-control';

let testIsPersistent = true;
let testUsername = `testUsername`;
let testReturnUrl = `testReturnUrl`;
let twoFactorAuthComponentFixture: ComponentFixture<TwoFactorAuthComponent>;
let twoFactorAuthComponent: TwoFactorAuthComponent;
let twoFactorAuthDebugElement: DebugElement;
let testTwoFactorAuthResponseModel: TwoFactorLogInResponseModel;
let testDynamicControl: DynamicControl;
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
        testDynamicControl = new DynamicControl({ name: `IsPersistent` });
        twoFactorAuthComponentFixture.detectChanges();
        stubDynamicFormComponent = twoFactorAuthComponent.dynamicFormComponent;
    });

    it(`Uses ViewChild to retrieve child DynamicFormsComponent and sets its IsPersistent DynamicControl's value`, () => {
        expect(stubDynamicFormComponent).toBeDefined();
        expect(testDynamicControl.value).toBe(testIsPersistent);
    });

    it(`Listens to child DynamicFormComponent output`, () => {
        spyOn(twoFactorAuthComponent, `onSubmitSuccess`);
        spyOn(twoFactorAuthComponent, `onSubmitError`);
        let anchorDebugElement = twoFactorAuthDebugElement.query(By.css(`a`));

        anchorDebugElement.triggerEventHandler('click', null);

        expect(twoFactorAuthComponent.onSubmitError).toHaveBeenCalledTimes(1);
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

    describe(`onSubmitError`, () => {
        it(`Sets codeExpired to true if twoFactorAuthResponseModel.expiredCredentials is true`, () => {
            testTwoFactorAuthResponseModel = { expiredCredentials: true }

            twoFactorAuthComponent.onSubmitError(testTwoFactorAuthResponseModel);

            expect(twoFactorAuthComponent.codeExpired).toBe(true);
        });

        it(`Sets codeExpired to true if twoFactorAuthResponseModel.expiredToken is true`, () => {
            testTwoFactorAuthResponseModel = { expiredToken: true }

            twoFactorAuthComponent.onSubmitError(testTwoFactorAuthResponseModel);

            expect(twoFactorAuthComponent.codeExpired).toBe(true);
        });
    });

    it(`Renders DynamicForm if codeExpired is false`, () => {
        twoFactorAuthComponent.codeExpired = false;
        twoFactorAuthComponentFixture.detectChanges();

        expect(twoFactorAuthDebugElement.queryAll(By.css(`dynamic-form`)).length).toBe(1);
        let divs = twoFactorAuthDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Code expired. Log in again to obtain a new code.`)).toBe(false);
        let anchors = twoFactorAuthDebugElement.queryAll(By.css(`a`));
        expect(anchors.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Log in`)).toBe(false);
    });

    it(`Renders code expired tip and log in link if codeExpired is true`, () => {
        twoFactorAuthComponent.codeExpired = true;
        twoFactorAuthComponentFixture.detectChanges();

        let divs = twoFactorAuthDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Code expired. Log in again to obtain a new code.`)).toBe(true);
        let anchors = twoFactorAuthDebugElement.queryAll(By.css(`a`));
        expect(anchors.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Log in`)).toBe(true);
        expect(twoFactorAuthDebugElement.queryAll(By.css(`dynamic-form`)).length).toBe(0);
    });
});

@Component({
    selector: `dynamic-form`,
    template: `<a (click)="submitSuccess.emit();submitError.emit()"></a>`
})
class StubDynamicFormComponent {
    @Output() submitSuccess = new EventEmitter<Response>();
    @Output() submitError = new EventEmitter<Response>();
    dynamicForm: DynamicForm = new DynamicForm([testDynamicControl], null, null);
}

class StubUserService {
    logIn(username: string, isPersistent: boolean): void {
    }

    returnUrl: string;
}