/* tslint:disable:no-unused-variable */
import { DebugElement, Component, Input, ViewChild} from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Observable } from 'rxjs';

import { DynamicControlComponent } from './dynamic-control.component';
import { DynamicFormsService } from '../dynamic-forms.service';
import { DynamicControl } from '../dynamic-control/dynamic-control';

let dynamicControlComponent: DynamicControlComponent;
let dynamicControl: DynamicControl<any>;
let stubHostComponent: StubHostComponent;
let stubHostFixture: ComponentFixture<StubHostComponent>;
let hostDebugElement: DebugElement;
let nativeElement: HTMLElement;

let testControlName = `testControlName`;
let testDisplayName = `testDisplayName`;
let testError = `testError`;
let inputTagName = `input`;
let selectTagName = `select`;

// Note: DynamicControlComponent uses the Renderer type. The test environment automatically substitutes 
// DebugDomRender for Renderer: https://angular.io/docs/ts/latest/guide/testing.html. Hence a Renderer provider cannot
// not need to be registered manually. To know whether Renderer functions get called, test the debug element it is called on.

describe('DynamicControlComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [DynamicControlComponent, StubHostComponent]
        }).compileComponents();
    }));

    beforeEach(() => {
        stubHostFixture = TestBed.createComponent(StubHostComponent);
        stubHostComponent = stubHostFixture.componentInstance;
        dynamicControl = stubHostComponent.dynamicControl;
        dynamicControlComponent = stubHostComponent.dynamicControlComponent;
        hostDebugElement = stubHostFixture.debugElement;
    });

    it(`Recieves input`, () => {
        stubHostFixture.detectChanges();

        expect(dynamicControlComponent.dynamicControl).toBe(stubHostComponent.dynamicControl);
    });

    it(`Renders and sets up control`, () => {
        dynamicControl.tagName = inputTagName;
        dynamicControl.properties = { testProperty: `testPropertyValue` };

        stubHostFixture.detectChanges();

        let controlDebugElement = hostDebugElement.query(By.css(inputTagName));
        expect(controlDebugElement).not.toBeNull();
        expect(controlDebugElement.listeners.length).toBe(2);
        expect(controlDebugElement.properties[`testProperty`]).toBe(`testPropertyValue`);
    });

    it(`Renders errors`, () => {
        dynamicControl.errors = [testError];

        stubHostFixture.detectChanges();

        expect(hostDebugElement.query(By.css(`span`)).nativeElement.textContent).toBe(testError);
    });

    it(`Renders label`, () => {
        dynamicControl.displayName = testDisplayName;

        stubHostFixture.detectChanges();

        expect(hostDebugElement.query(By.css(`label`)).nativeElement.textContent).toBe(testDisplayName);
    });
});

let submitEventStub = {
    preventDefault: () => null
};

@Component({
    template: `<dynamic-control [dynamicControl]="dynamicControl"></dynamic-control>`
})
class StubHostComponent {
    dynamicControl = new DynamicControl<any>({});
    @ViewChild(DynamicControlComponent) dynamicControlComponent: DynamicControlComponent;
}
