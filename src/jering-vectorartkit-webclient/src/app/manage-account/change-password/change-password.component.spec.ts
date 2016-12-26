import { Component, Output, EventEmitter, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router } from '@angular/router';

import { AppPaths } from 'app/app.paths';
import { ChangePasswordComponent } from './change-password.component';

import { StubDynamicFormComponent } from 'testing/dynamic-form.component.stub';
import { StubRouter } from 'testing/router-stubs';

let testSubmitSuccessElementId = 'testSubmitSuccessElementId';
let changePasswordComponentFixture: ComponentFixture<ChangePasswordComponent>;
let changePasswordComponent: ChangePasswordComponent;
let changePasswordDebugElement: DebugElement;
let stubRouter: StubRouter;

describe('ChangePasswordComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ChangePasswordComponent, StubDynamicFormComponent],
            providers: [{ provide: Router, useClass: StubRouter}]
        }).compileComponents();
    }));

    beforeEach(() => {
        changePasswordComponentFixture = TestBed.createComponent(ChangePasswordComponent);
        changePasswordComponent = changePasswordComponentFixture.componentInstance;
        changePasswordDebugElement = changePasswordComponentFixture.debugElement;
        stubRouter = TestBed.get(Router) as StubRouter;
    });

    it(`Listens to child DynamicFormComponent outputs`, () => {
        changePasswordComponentFixture.detectChanges();
        spyOn(changePasswordComponent, `onSubmitSuccess`);
        let anchorDebugElements = changePasswordDebugElement.queryAll(By.css(`a`));

        anchorDebugElements.forEach(debugElement => debugElement.triggerEventHandler('click', null));

        expect(changePasswordComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    it(`onSubmitSuccess sets calls Router.navigate`, () => {
        spyOn(stubRouter, `navigate`);

        changePasswordComponent.onSubmitSuccess(null);

        expect(stubRouter.navigate).toHaveBeenCalledWith([AppPaths.manageAccountPath]);
    });
});
