/* tslint:disable:no-unused-variable */
/* tslint:disable:no-unused-variable */
import { DebugElement, Component, Input, ViewChild } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Observable } from 'rxjs';

import { DynamicFormComponent } from './dynamic-form.component';
import { DynamicFormsService } from '../dynamic-forms.service';
import { DynamicControl } from '../dynamic-control/dynamic-control';
import { DynamicForm } from './dynamic-form';

let dynamicFormComponent: DynamicFormComponent;
let hostComponentStub: StubHostComponent;
let hostFixtureStub: ComponentFixture<StubHostComponent>;
let hostDebugElement: DebugElement;
let nativeElement: HTMLElement;

let testControlName = `testControlName`;
let testFormModelName = `testFormModelName`;
let testDynamicForm = new DynamicForm([new DynamicControl<any>({ name: testControlName })]);
let stubSubmitEvent = {
    preventDefault: () => null
};

describe('DynamicFormComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [DynamicFormComponent, StubDynamicControlComponent, StubHostComponent],
            providers: [{ provide: DynamicFormsService, useClass: StubDynamicFormsService }]
        }).compileComponents();
    }));

    beforeEach(() => {
        hostFixtureStub = TestBed.createComponent(StubHostComponent);
        hostComponentStub = hostFixtureStub.componentInstance;
        dynamicFormComponent = hostComponentStub.dynamicFormComponent;
        hostDebugElement = hostFixtureStub.debugElement;
    });

    it(`Recieves input`, () => {
        hostFixtureStub.detectChanges();

        expect(dynamicFormComponent.formModelName).toBe(hostComponentStub.formModelName);
    });

    it(`Initializes`, () => {
        let dynamicFormsServiceStub = TestBed.get(DynamicFormsService) as StubDynamicFormsService;
        spyOn(dynamicFormsServiceStub, `getDynamicForm`).and.callThrough();

        hostFixtureStub.detectChanges();

        expect(dynamicFormsServiceStub.getDynamicForm).toHaveBeenCalledWith(testFormModelName);
        expect(dynamicFormComponent.dynamicForm).toBe(testDynamicForm);
    });

    it(`Displays DynamicForm`, () => {
        hostFixtureStub.detectChanges();

        expect(hostDebugElement.query(By.css(`dynamic-control`)).nativeElement.textContent).
            toBe(testControlName);
    });

    it(`Registers submit event listener`, () => {
        hostFixtureStub.detectChanges();

        spyOn(dynamicFormComponent, `onSubmit`).and.callThrough();

        let formDebugElement = hostDebugElement.query(By.css(`form`));
        formDebugElement.triggerEventHandler('submit', stubSubmitEvent);

        expect(dynamicFormComponent.onSubmit).toHaveBeenCalledWith(stubSubmitEvent);
        // ensure that service http submission is called
    });
});

class StubDynamicFormsService {
    getDynamicForm(formModelName: string): Observable<DynamicForm> {
        return Observable.of(testDynamicForm);
    };
}

@Component({
    template: `<dynamic-form [formModelName]="formModelName"></dynamic-form>`
})
class StubHostComponent {
    @ViewChild(DynamicFormComponent) dynamicFormComponent: DynamicFormComponent;
    formModelName = testFormModelName;
}

@Component({
    selector: `dynamic-control`,
    template: `{{dynamicControl.name}}`
})
class StubDynamicControlComponent {
    @Input() dynamicControl: DynamicControl<any>;
}
