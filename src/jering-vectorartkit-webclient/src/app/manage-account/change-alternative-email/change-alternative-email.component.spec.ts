import { Component, Output, EventEmitter, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router } from '@angular/router';

import { ChangeAlternativeEmailComponent } from './change-alternative-email.component';
import { StubRouter } from '../../../testing/router-stubs';

let testSubmitSuccessElementId = "testSubmitSuccessElementId";
let changeAlternativeEmailComponentFixture: ComponentFixture<ChangeAlternativeEmailComponent>;
let changeAlternativeEmailComponent: ChangeAlternativeEmailComponent;
let changeAlternativeEmailDebugElement: DebugElement;
let stubRouter: StubRouter;

describe('ChangeAlternativeEmailComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ChangeAlternativeEmailComponent, StubDynamicFormComponent],
            providers: [{ provide: Router, useClass: StubRouter}]
        }).compileComponents();
    }));

    beforeEach(() => {
        changeAlternativeEmailComponentFixture = TestBed.createComponent(ChangeAlternativeEmailComponent);
        changeAlternativeEmailComponent = changeAlternativeEmailComponentFixture.componentInstance;
        changeAlternativeEmailDebugElement = changeAlternativeEmailComponentFixture.debugElement;
        stubRouter = TestBed.get(Router) as StubRouter;
        changeAlternativeEmailComponentFixture.detectChanges();
    });

    it(`Listens to child DynamicFormComponent outputs`, () => {
        spyOn(changeAlternativeEmailComponent, `onSubmitSuccess`);

        changeAlternativeEmailDebugElement.
            query(By.css(`#${testSubmitSuccessElementId}`)).
            triggerEventHandler('click', null);

        expect(changeAlternativeEmailComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    it(`onSubmitSuccess sets calls Router.navigate`, () => {
        spyOn(stubRouter, `navigate`);

        changeAlternativeEmailComponent.onSubmitSuccess(null);

        expect(stubRouter.navigate).toHaveBeenCalledWith([`/manage-account`]);
    });
});

@Component({
    selector: `dynamic-form`,
    template: `<a id=${testSubmitSuccessElementId} (click)=submitSuccess.emit()></a>`
})
class StubDynamicFormComponent {
    @Output() submitSuccess = new EventEmitter<any>();
}