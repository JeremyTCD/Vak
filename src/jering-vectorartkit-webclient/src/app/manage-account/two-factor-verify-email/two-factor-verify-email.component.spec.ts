import { Component, Output, EventEmitter, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router } from '@angular/router';

import { AppPaths } from 'app/app.paths';
import { TwoFactorVerifyEmailComponent } from './two-factor-verify-email.component';

import { StubRouter } from 'testing/router-stubs';
import { StubDynamicFormComponent } from 'testing/dynamic-form.component.stub';

let twoFactorVerifyEmailComponentFixture: ComponentFixture<TwoFactorVerifyEmailComponent>;
let twoFactorVerifyEmailComponent: TwoFactorVerifyEmailComponent;
let twoFactorVerifyEmailDebugElement: DebugElement;
let stubRouter: StubRouter;

describe('TwoFactorVerifyEmailComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [TwoFactorVerifyEmailComponent, StubDynamicFormComponent],
            providers: [{ provide: Router, useClass: StubRouter }]
        }).compileComponents();
    }));

    beforeEach(() => {
        twoFactorVerifyEmailComponentFixture = TestBed.createComponent(TwoFactorVerifyEmailComponent);
        twoFactorVerifyEmailComponent = twoFactorVerifyEmailComponentFixture.componentInstance;
        twoFactorVerifyEmailDebugElement = twoFactorVerifyEmailComponentFixture.debugElement;
        stubRouter = TestBed.get(Router) as StubRouter;
    });

    it(`Listens to child DynamicFormComponent outputs`, () => {
        twoFactorVerifyEmailComponentFixture.detectChanges();
        spyOn(twoFactorVerifyEmailComponent, `onSubmitSuccess`);
        let anchorDebugElements = twoFactorVerifyEmailDebugElement.queryAll(By.css(`a`));

        anchorDebugElements.forEach(debugElement => debugElement.triggerEventHandler('click', null));

        expect(twoFactorVerifyEmailComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    it(`onSubmitSuccess sets calls Router.navigate`, () => {
        spyOn(stubRouter, `navigate`);

        twoFactorVerifyEmailComponent.onSubmitSuccess(null);

        expect(stubRouter.navigate).toHaveBeenCalledWith([AppPaths.manageAccountPath]);
    });
});
