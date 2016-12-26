import { Component, Output, EventEmitter, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router } from '@angular/router';

import { AppPaths } from 'app/app.paths';

import { ChangeDisplayNameComponent } from './change-display-name.component';
import { StubRouter } from 'testing/router-stubs';
import { StubDynamicFormComponent } from 'testing/dynamic-form.component.stub';

let testSubmitSuccessElementId = 'testSubmitSuccessElementId';
let changeDisplayNameComponentFixture: ComponentFixture<ChangeDisplayNameComponent>;
let changeDisplayNameComponent: ChangeDisplayNameComponent;
let changeDisplayNameDebugElement: DebugElement;
let stubRouter: StubRouter;

describe('ChangeDisplayNameComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ChangeDisplayNameComponent, StubDynamicFormComponent],
            providers: [{ provide: Router, useClass: StubRouter}]
        }).compileComponents();
    }));

    beforeEach(() => {
        changeDisplayNameComponentFixture = TestBed.createComponent(ChangeDisplayNameComponent);
        changeDisplayNameComponent = changeDisplayNameComponentFixture.componentInstance;
        changeDisplayNameDebugElement = changeDisplayNameComponentFixture.debugElement;
        stubRouter = TestBed.get(Router) as StubRouter;
    });

    it(`Listens to child DynamicFormComponent outputs`, () => {
        changeDisplayNameComponentFixture.detectChanges();
        spyOn(changeDisplayNameComponent, `onSubmitSuccess`);

        let anchorDebugElements = changeDisplayNameDebugElement.queryAll(By.css(`a`));

        anchorDebugElements.forEach(debugElement => debugElement.triggerEventHandler('click', null));


        expect(changeDisplayNameComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    it(`onSubmitSuccess sets calls Router.navigate`, () => {
        spyOn(stubRouter, `navigate`);

        changeDisplayNameComponent.onSubmitSuccess(null);

        expect(stubRouter.navigate).toHaveBeenCalledWith([AppPaths.manageAccountPath]);
    });
});
