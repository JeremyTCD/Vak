import { Component, Output, EventEmitter, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router } from '@angular/router';

import { ChangeAltEmailComponent } from './change-alt-email.component';
import { AppPaths } from 'app/app.paths';

import { StubRouter } from 'testing/router-stubs';
import { StubDynamicFormComponent } from 'testing/dynamic-form.component.stub';

let testSubmitSuccessElementId = 'testSubmitSuccessElementId';
let changeAltEmailComponentFixture: ComponentFixture<ChangeAltEmailComponent>;
let changeAltEmailComponent: ChangeAltEmailComponent;
let changeAltEmailDebugElement: DebugElement;
let stubRouter: StubRouter;

describe('ChangeAltEmailComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ChangeAltEmailComponent, StubDynamicFormComponent],
            providers: [{ provide: Router, useClass: StubRouter}]
        }).compileComponents();
    }));

    beforeEach(() => {
        changeAltEmailComponentFixture = TestBed.createComponent(ChangeAltEmailComponent);
        changeAltEmailComponent = changeAltEmailComponentFixture.componentInstance;
        changeAltEmailDebugElement = changeAltEmailComponentFixture.debugElement;
        stubRouter = TestBed.get(Router) as StubRouter;
    });

    it(`Listens to child DynamicFormComponent outputs`, () => {
        changeAltEmailComponentFixture.detectChanges();
        spyOn(changeAltEmailComponent, `onSubmitSuccess`);
        let anchorDebugElements = changeAltEmailDebugElement.queryAll(By.css(`a`));

        anchorDebugElements.forEach(debugElement => debugElement.triggerEventHandler('click', null));

        expect(changeAltEmailComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    it(`onSubmitSuccess sets calls Router.navigate`, () => {
        spyOn(stubRouter, `navigate`);

        changeAltEmailComponent.onSubmitSuccess(null);

        expect(stubRouter.navigate).toHaveBeenCalledWith([AppPaths.manageAccountPath]);
    });
});