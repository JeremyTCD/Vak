/* tslint:disable:no-unused-variable */
/* tslint:disable:no-unused-variable */
import { DebugElement, Component, Input, ViewChild } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Observable } from 'rxjs';
import { Router } from '@angular/router/index';
import { Response, ResponseOptions } from '@angular/http';

import { DynamicFormComponent } from './dynamic-form.component';
import { DynamicFormsService } from '../dynamic-forms.service';
import { DynamicControl } from '../dynamic-control/dynamic-control';
import { DynamicForm } from './dynamic-form';
import { Validity } from '../validity';
import { ErrorHandlerService } from '../../utility/error-handler.service';
import { StubDomEvent } from '../../../../testing/dom-stubs';
import { StubRouter } from '../../../../testing/router-stubs';

let dynamicFormComponent: DynamicFormComponent;
let stubHostComponent: StubHostComponent;
let stubHostFixture: ComponentFixture<StubHostComponent>;
let hostDebugElement: DebugElement;
let nativeElement: HTMLElement;
let stubDynamicFormsService: StubDynamicFormsService;
let stubErrorHandlerService: StubErrorHandlerService;

let testControlName = `testControlName`;
let testFormModelName = `testFormModelName`;
let testSubmitUrl = `testSubmitUrl`;
let testMessage = `testErrorMessage`;
let testDynamicControl: DynamicControl<any>;
let testDynamicForm: DynamicForm;
let testSubmitEvent: StubDomEvent;
let testResponse: Response;

describe('DynamicFormComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [DynamicFormComponent, StubDynamicControlComponent, StubHostComponent],
            providers: [{ provide: DynamicFormsService, useClass: StubDynamicFormsService },
            { provide: Router, useClass: StubRouter },
            { provide: ErrorHandlerService, useClass: StubErrorHandlerService }]
        }).compileComponents();
    }));

    beforeEach(() => {
        stubHostFixture = TestBed.createComponent(StubHostComponent);
        stubHostComponent = stubHostFixture.componentInstance;
        dynamicFormComponent = stubHostComponent.dynamicFormComponent;
        hostDebugElement = stubHostFixture.debugElement;
        stubDynamicFormsService = TestBed.get(DynamicFormsService) as StubDynamicFormsService;
        stubErrorHandlerService = TestBed.get(ErrorHandlerService) as StubErrorHandlerService;
        testDynamicControl = new DynamicControl<any>({ name: testControlName });
        testDynamicForm = new DynamicForm([testDynamicControl], testMessage);
        testSubmitEvent = new StubDomEvent();
        testResponse = new Response(
            new ResponseOptions({
                body: ``
            })
        );
    });

    it(`Emits output`, () => {
        stubHostFixture.detectChanges();

        spyOn(stubHostComponent, `onSubmitSuccess`);

        dynamicFormComponent.submitSuccess.emit(testResponse);

        expect(stubHostComponent.onSubmitSuccess).toHaveBeenCalledWith(testResponse);
    });

    it(`Recieves input`, () => {
        stubHostFixture.detectChanges();

        expect(dynamicFormComponent.formModelName).toBe(stubHostComponent.formModelName);
        expect(dynamicFormComponent.formSubmitUrl).toBe(stubHostComponent.formSubmitUrl);
    });

    describe(`Oninit`, () => {
        it(`Calls DynamicFormsService.getDynamicForm`, () => {
            spyOn(stubDynamicFormsService, `getDynamicForm`).and.returnValue(Observable.empty<DynamicForm>());

            stubHostFixture.detectChanges();

            expect(stubDynamicFormsService.getDynamicForm).toHaveBeenCalled();

        });

        it(`Sets dynamicForm if DynamicFormsService.getDynamicForm succeeds`, () => {
            spyOn(stubDynamicFormsService, `getDynamicForm`).and.returnValue(Observable.of(testDynamicForm));

            stubHostFixture.detectChanges();

            expect(dynamicFormComponent.dynamicForm).toBe(testDynamicForm);
        });
    });

    it(`Displays DynamicForm DynamicControls`, () => {
        stubHostFixture.detectChanges();

        expect(hostDebugElement.query(By.css(`dynamic-control`)).nativeElement.textContent).
            toBe(testControlName);
    });

    it(`Displays DynamicForm messages`, () => {
        testDynamicForm.messages.push(testDynamicForm.message);

        stubHostFixture.detectChanges();

        expect(hostDebugElement.query(By.css(`span`)).nativeElement.textContent).
            toBe(testMessage);
    });

    it(`submit event calls onSubmit`, () => {
        let formDebugElement: DebugElement;
        stubHostFixture.detectChanges();
        formDebugElement = hostDebugElement.query(By.css(`form`));
        spyOn(dynamicFormComponent, `onSubmit`);

        formDebugElement.triggerEventHandler('submit', testSubmitEvent);

        expect(dynamicFormComponent.onSubmit).toHaveBeenCalledTimes(1);
    });

    describe(`onSubmit`, () => {
        beforeEach(() => {
            // OnInit and input from a host component initialize these fields in production.
            // In this case they must be manually set to limit test scope.
            dynamicFormComponent.dynamicForm = testDynamicForm;
            dynamicFormComponent.formSubmitUrl = testSubmitUrl;
        });

        it(`Calls DynamicFormServices.submitDynamicForm if DynamicForm.onSubmit is true`, () => {
            spyOn(testDynamicForm, `onSubmit`).and.returnValue(true);
            spyOn(stubDynamicFormsService, `submitDynamicForm`).and.returnValue(Observable.empty<Response>());

            dynamicFormComponent.onSubmit(testSubmitEvent);

            expect(testDynamicForm.onSubmit).toHaveBeenCalled();
            expect(stubDynamicFormsService.submitDynamicForm).toHaveBeenCalledWith(testSubmitUrl, testDynamicForm);
        });

        it(`Emits submitSuccess event if DynamicFormServices.submitDynamicForm succeeds`, () => {
            spyOn(testDynamicForm, `onSubmit`).and.returnValue(true);
            spyOn(stubDynamicFormsService, `submitDynamicForm`).and.returnValue(Observable.of(testResponse));
            spyOn(dynamicFormComponent.submitSuccess, `emit`);

            dynamicFormComponent.onSubmit(testSubmitEvent);

            expect(dynamicFormComponent.submitSuccess.emit).toHaveBeenCalled();
        });

        it(`If DynamicFormServices.submitDynamicForm fails, for each child DynamicControl with errors, adds model state error messages 
            to DynamicControl.messages and sets DynamicControl.validity to Validity.invalid. Also adds DynamicForm.message to 
            DynamicFormmessages and sets DynamicFormvalidity to Validity.invalid.`, () => {
                spyOn(testDynamicForm, `onSubmit`).and.returnValue(true);
                spyOn(stubDynamicFormsService, `submitDynamicForm`).and.returnValue(Observable.throw({ testControlName: [testMessage] }));

                dynamicFormComponent.onSubmit(testSubmitEvent);

                expect(testDynamicControl.validity).toBe(Validity.invalid);
                expect(testDynamicControl.messages).toEqual([testMessage]);
                expect(testDynamicForm.validity).toBe(Validity.invalid);
                expect(testDynamicForm.messages).toEqual([testMessage]);
            });
    });
});

class StubErrorHandlerService {
    handleUnexpectedError(router: Router, error: any): void {

    }
}

class StubDynamicFormsService {
    getDynamicForm(formModelName: string): Observable<DynamicForm> {
        return Observable.of(testDynamicForm);
    };

    submitDynamicForm(url: string, dynamicForm: DynamicForm): Observable<Response> {
        return Observable.of(testResponse);
    }
}

@Component({
    template: `<dynamic-form [formModelName]="formModelName" [formSubmitUrl]="formSubmitUrl" (submitSuccess)="onSubmitSuccess($event)" (submitError)="onSubmitError($event)"></dynamic-form>`
})
class StubHostComponent {
    @ViewChild(DynamicFormComponent) dynamicFormComponent: DynamicFormComponent;
    formModelName = testFormModelName;
    formSubmitUrl = testSubmitUrl;

    onSubmitSuccess(response: Response): void {
    }

    onSubmitError(response: Response): void {
    }
}

@Component({
    selector: `dynamic-control`,
    template: `{{dynamicControl.name}}`
})
class StubDynamicControlComponent {
    @Input() dynamicControl: DynamicControl<any>;
}
