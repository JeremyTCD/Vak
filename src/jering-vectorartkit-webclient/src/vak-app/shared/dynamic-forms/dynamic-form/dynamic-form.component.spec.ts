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
import { ErrorHandlerService } from '../../utility/error-handler.service';
import { StubDomEvent } from '../../../../testing/dom-stubs';

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
let testDynamicForm = new DynamicForm([new DynamicControl<any>({ name: testControlName })], testMessage);
let testSubmitEvent = new StubDomEvent();
let testResponse = new Response(
    new ResponseOptions({
        body: ``
    })
);

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
    });

    it(`Emits output`, () => {
        stubHostFixture.detectChanges();

        spyOn(stubHostComponent, `onSubmitSuccess`);
        spyOn(stubHostComponent, `onSubmitError`);

        dynamicFormComponent.submitSuccess.emit(testResponse);
        dynamicFormComponent.submitError.emit(testResponse);

        expect(stubHostComponent.onSubmitSuccess).toHaveBeenCalledWith(testResponse);
        expect(stubHostComponent.onSubmitError).toHaveBeenCalledWith(testResponse);
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

        it(`Calls ErrorHandlerService.handleUnexpectedError if DynamicFormsService.getDynamicForm fails`, () => {
            spyOn(stubDynamicFormsService, `getDynamicForm`).and.returnValue(Observable.throw(null));
            spyOn(stubErrorHandlerService, `handleUnexpectedError`);

            stubHostFixture.detectChanges();

            expect(stubErrorHandlerService.handleUnexpectedError).toHaveBeenCalledTimes(1);
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

    describe(`Submit event`, () => {
        let formDebugElement: DebugElement;

        beforeEach(() => {
            stubHostFixture.detectChanges();
            formDebugElement = hostDebugElement.query(By.css(`form`));
        });

        it(`Calls onSubmit, which calls DynamicFormServices.submitDynamicForm if DynamicForm.onSubmit is true`, () => {
            spyOn(dynamicFormComponent, `onSubmit`).and.callThrough();
            spyOn(testDynamicForm, `onSubmit`).and.returnValue(true);
            spyOn(stubDynamicFormsService, `submitDynamicForm`).and.returnValue(Observable.empty<Response>());

            formDebugElement.triggerEventHandler('submit', testSubmitEvent);

            expect(dynamicFormComponent.onSubmit).toHaveBeenCalledWith(testSubmitEvent);
            expect(testDynamicForm.onSubmit).toHaveBeenCalled();
            expect(stubDynamicFormsService.submitDynamicForm).toHaveBeenCalledWith(testSubmitUrl, testDynamicForm);
        });

        it(`Emits submitSuccess event if DynamicFormServices.submitDynamicForm succeeds`, () => {
            spyOn(stubDynamicFormsService, `submitDynamicForm`).and.returnValue(Observable.of(testResponse));
            spyOn(dynamicFormComponent.submitSuccess, `emit`);

            formDebugElement.triggerEventHandler('submit', testSubmitEvent);

            expect(dynamicFormComponent.submitSuccess.emit).toHaveBeenCalledWith(testResponse);
        });

        it(`Emits submitError event if DynamicFormServices.submitDynamicForm fails`, () => {
            spyOn(stubDynamicFormsService, `submitDynamicForm`).and.returnValue(Observable.throw(testResponse));
            spyOn(dynamicFormComponent.submitError, `emit`);

            formDebugElement.triggerEventHandler('submit', testSubmitEvent);

            expect(dynamicFormComponent.submitError.emit).toHaveBeenCalledWith(testResponse);
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

class StubRouter {
    navigate(commands: any[]): void {
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
