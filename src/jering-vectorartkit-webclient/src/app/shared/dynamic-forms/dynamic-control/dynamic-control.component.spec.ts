/* tslint:disable:no-unused-variable */
import { DebugElement, Component, Input, ViewChild} from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Observable } from 'rxjs';

import { DynamicControlComponent } from './dynamic-control.component';
import { DynamicFormsService } from '../dynamic-forms.service';
import { DynamicControl } from './dynamic-control';
import { Validity } from '../validity';

let dynamicControlComponent: DynamicControlComponent;
let dynamicControl: DynamicControl;
let stubHostComponent: StubHostComponent;
let stubHostFixture: ComponentFixture<StubHostComponent>;
let hostDebugElement: DebugElement;
let nativeElement: HTMLElement;

let testPropertyValue = `testPropertyValue`;
let testControlName = `testControlName`;
let testDisplayName = `testDisplayName`;
let testMessage = `testMessage`;
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
        dynamicControl.tagName = inputTagName;
        dynamicControlComponent = stubHostComponent.dynamicControlComponent;
        hostDebugElement = stubHostFixture.debugElement;
    });

    it(`Recieves input`, () => {
        stubHostFixture.detectChanges();

        expect(dynamicControlComponent.dynamicControl).toBe(stubHostComponent.dynamicControl);
    });

    it(`Renders control`, () => {
        dynamicControl.properties = { type: `email`, testProperty: testPropertyValue };
        dynamicControl.name = testControlName;

        stubHostFixture.detectChanges();

        let controlDebugElement = hostDebugElement.query(By.css(inputTagName));
        expect(controlDebugElement).not.toBeNull();
        expect(controlDebugElement.listeners.length).toBe(2);
        expect(controlDebugElement.properties[`testProperty`]).toBe(testPropertyValue);
        expect(controlDebugElement.properties[`id`]).toBe(testControlName);
        expect(controlDebugElement.classes[Validity[Validity.unknown]]).toBe(true);
    });

    it(`Renders messages`, () => {
        dynamicControl.messages = [testMessage];

        stubHostFixture.detectChanges();

        expect(hostDebugElement.query(By.css(`span`)).nativeElement.textContent).toBe(testMessage);
    });

    it(`Renders label if display name is defined`, () => {
        dynamicControl.displayName = testDisplayName;

        stubHostFixture.detectChanges();

        expect(hostDebugElement.query(By.css(`label`)).nativeElement.textContent).toBe(testDisplayName);
    });

    it(`Does not render label if display name is not defined`, () => {
        dynamicControl.displayName = undefined;

        stubHostFixture.detectChanges();

        expect(hostDebugElement.query(By.css(`label`))).toBe(null);
    });

    it(`Renders pending notification`, () => {
        dynamicControl.validity = Validity.pending;

        stubHostFixture.detectChanges();

        let pendingDebugElement = hostDebugElement.query((debugElement: DebugElement, index: number, array: DebugElement[]) => {
            if (debugElement.nativeElement.textContent === `Pending`) {
                return true;
            }
            return false;
        })

        expect(pendingDebugElement).toBeDefined();
    });
});

@Component({
    template: `<dynamic-control [dynamicControl]="dynamicControl"></dynamic-control>`
})
class StubHostComponent {
    dynamicControl = new DynamicControl({});
    @ViewChild(DynamicControlComponent) dynamicControlComponent: DynamicControlComponent;
}
