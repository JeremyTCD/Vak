import { Component, Output, EventEmitter, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { Router } from '@angular/router';

import { SetEmailRequestModel } from 'api/request-models/set-email.request-model';

import { StubRouter } from 'testing/router-stubs';
import { StubUserService } from 'testing/user.service.stub';
import { StubDynamicFormComponent } from 'testing/dynamic-form.component.stub';

import { ChangeEmailComponent } from './change-email.component';
import { UserService } from 'app/shared/user.service';
import { SubmitEventModel } from 'app/shared/dynamic-forms/dynamic-form/submit-event.model';
import { AppPaths } from 'app/app.paths';


let testNewUsername = `testNewUsername`;
let testSubmitSuccessElementId = `testSubmitSuccessElementId`;
let changeEmailComponentFixture: ComponentFixture<ChangeEmailComponent>;
let changeEmailComponent: ChangeEmailComponent;
let changeEmailDebugElement: DebugElement;
let stubRouter: StubRouter;
let stubUserService: StubUserService;

describe('ChangeEmailComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ChangeEmailComponent, StubDynamicFormComponent],
            providers: [{ provide: Router, useClass: StubRouter },
                { provide: UserService, useClass: StubUserService }]
        }).compileComponents();
    }));

    beforeEach(() => {
        changeEmailComponentFixture = TestBed.createComponent(ChangeEmailComponent);
        changeEmailComponent = changeEmailComponentFixture.componentInstance;
        changeEmailDebugElement = changeEmailComponentFixture.debugElement;
        stubRouter = TestBed.get(Router) as StubRouter;
        stubUserService = TestBed.get(UserService) as StubUserService;
    });

    it(`Listens to child DynamicFormComponent outputs`, () => {
        changeEmailComponentFixture.detectChanges();
        spyOn(changeEmailComponent, `onSubmitSuccess`);
        let anchorDebugElements = changeEmailDebugElement.queryAll(By.css(`a`));

        anchorDebugElements.forEach(debugElement => debugElement.triggerEventHandler('click', null));

        expect(changeEmailComponent.onSubmitSuccess).toHaveBeenCalledTimes(1);
    });

    it(`onSubmitSuccess sets calls UserService.changeUsername and Router.navigate`, () => {
        spyOn(stubRouter, `navigate`);
        spyOn(stubUserService, `changeUsername`);
        let requestModel: SetEmailRequestModel = { newEmail: testNewUsername };
        let eventModel: SubmitEventModel = new SubmitEventModel(null, requestModel);

        changeEmailComponent.onSubmitSuccess(eventModel);

        expect(stubUserService.changeUsername).toHaveBeenCalledWith(testNewUsername);
        expect(stubRouter.navigate).toHaveBeenCalledWith([AppPaths.manageAccountPath]);
    });
});
