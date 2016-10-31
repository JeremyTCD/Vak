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
import { StubDomEvent } from '../../../../testing/dom-stubs';

let dynamicFormComponent: DynamicFormComponent;
let stubHostComponent: StubHostComponent;
let stubHostFixture: ComponentFixture<StubHostComponent>;
let hostDebugElement: DebugElement;
let nativeElement: HTMLElement;
let stubDynamicFormsService; 

let testControlName = `testControlName`;
let testFormModelName = `testFormModelName`;
let testSubmitUrl = `testSubmitUrl`;
let testErrorMessage = `testErrorMessage`;
let testDynamicForm = new DynamicForm([new DynamicControl<any>({ name: testControlName })], testErrorMessage);
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
                { provide: Router, useClass: StubRouter }]
        }).compileComponents();
    }));

    beforeEach(() => {
        stubHostFixture = TestBed.createComponent(StubHostComponent);
        stubHostComponent = stubHostFixture.componentInstance;
        dynamicFormComponent = stubHostComponent.dynamicFormComponent;
        hostDebugElement = stubHostFixture.debugElement;
        stubDynamicFormsService = TestBed.get(DynamicFormsService) as StubDynamicFormsService;
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

    it(`Initializes`, () => {
        spyOn(stubDynamicFormsService, `getDynamicForm`).and.callThrough();

        stubHostFixture.detectChanges();

        expect(stubDynamicFormsService.getDynamicForm).toHaveBeenCalledWith(testFormModelName);
        expect(dynamicFormComponent.dynamicForm).toBe(testDynamicForm);
    });

    it(`Displays DynamicForm`, () => {
        stubHostFixture.detectChanges();

        expect(hostDebugElement.query(By.css(`dynamic-control`)).nativeElement.textContent).
            toBe(testControlName);
    });

    it(`Listens to form submit event`, () => {
        stubHostFixture.detectChanges();

        spyOn(dynamicFormComponent, `onSubmit`).and.callThrough();
        spyOn(testDynamicForm, `onSubmit`).and.returnValue(true);
        spyOn(stubDynamicFormsService, `submitDynamicForm`).and.callThrough();

        let formDebugElement = hostDebugElement.query(By.css(`form`));
        formDebugElement.triggerEventHandler('submit', testSubmitEvent);

        expect(dynamicFormComponent.onSubmit).toHaveBeenCalledWith(testSubmitEvent);
        expect(testDynamicForm.onSubmit).toHaveBeenCalled();
        expect(stubDynamicFormsService.submitDynamicForm).toHaveBeenCalledWith(testSubmitUrl, testDynamicForm);

        // ensure that service http submission is called
    });
});

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
