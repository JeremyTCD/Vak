import { Component, Output, EventEmitter, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';

import { VerifyEmailComponent } from './verify-email.component';
import { SetEmailVerifiedResponseModel } from 'api/response-models/set-email-verified.response-model';
import { StubActivatedRoute } from 'testing/router-stubs';
import { DebugElementExtensions } from 'testing/debug-element-extensions';

let verifyEmailComponentFixture: ComponentFixture<VerifyEmailComponent>;
let verifyEmailComponent: VerifyEmailComponent;
let verifyEmailDebugElement: DebugElement;
let stubActivatedRoute: StubActivatedRoute;

describe('VerifyEmailComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [VerifyEmailComponent],
            providers: [{ provide: ActivatedRoute, useClass: StubActivatedRoute }]
        }).compileComponents();
    }));

    beforeEach(() => {
        verifyEmailComponentFixture = TestBed.createComponent(VerifyEmailComponent);
        verifyEmailComponent = verifyEmailComponentFixture.componentInstance;
        verifyEmailDebugElement = verifyEmailComponentFixture.debugElement;
        stubActivatedRoute = TestBed.get(ActivatedRoute) as StubActivatedRoute;
    });

    describe(`ngOnInit`, () => {
        it(`ngOnInit sets linkExpiredOrInvalid to true if responseModel.invalidToken is true`, () => {
            let responseModel: SetEmailVerifiedResponseModel = { invalidToken: true };
            stubActivatedRoute.testData = { responseModel: responseModel };

            verifyEmailComponentFixture.detectChanges();

            expect(verifyEmailComponent.linkExpiredOrInvalid).toBe(true);
        });

        it(`ngOnInit does not set linkExpiredOrInvalid to true if responseModel.invalidToken is false`, () => {
            let responseModel: SetEmailVerifiedResponseModel = { invalidToken: false };
            stubActivatedRoute.testData = { responseModel: responseModel };

            verifyEmailComponentFixture.detectChanges();

            expect(verifyEmailComponent.linkExpiredOrInvalid).toBe(false);
        });
    });

    it(`Renders link expired or invalid tip if linkExpiredOrInvalid is true`, () => {
        let responseModel: SetEmailVerifiedResponseModel = { invalidToken: true };
        stubActivatedRoute.testData = { responseModel: responseModel };
        verifyEmailComponentFixture.detectChanges();

        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(verifyEmailDebugElement, `Link expired or invalid`)).toBe(true);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(verifyEmailDebugElement, `Get a new link`)).toBe(true);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(verifyEmailDebugElement, `Your email has been verified`)).toBe(false);
    });

    it(`Renders alternative email has been verified tip if linkExpiredOrInvalid is false`, () => {
        let responseModel: SetEmailVerifiedResponseModel = { invalidToken: false };
        stubActivatedRoute.testData = { responseModel: responseModel };
        verifyEmailComponentFixture.detectChanges();

        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(verifyEmailDebugElement, `Your email has been verified`)).toBe(true);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(verifyEmailDebugElement, `Link expired or invalid`)).toBe(false);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(verifyEmailDebugElement, `Get a new link`)).toBe(false);
    });
});
