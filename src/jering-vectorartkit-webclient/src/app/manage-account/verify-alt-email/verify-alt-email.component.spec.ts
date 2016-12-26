import { Component, Output, EventEmitter, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';

import { VerifyAltEmailComponent } from './verify-alt-email.component';
import { SetAltEmailVerifiedResponseModel } from 'api/response-models/set-alt-email-verified.response-model';
import { StubActivatedRoute } from 'testing/router-stubs';
import { DebugElementExtensions } from 'testing/debug-element-extensions';

let verifyAltEmailComponentFixture: ComponentFixture<VerifyAltEmailComponent>;
let verifyAltEmailComponent: VerifyAltEmailComponent;
let verifyAltEmailDebugElement: DebugElement;
let stubActivatedRoute: StubActivatedRoute;

describe('VerifyAltEmailComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [VerifyAltEmailComponent],
            providers: [{ provide: ActivatedRoute, useClass: StubActivatedRoute }]
        }).compileComponents();
    }));

    beforeEach(() => {
        verifyAltEmailComponentFixture = TestBed.createComponent(VerifyAltEmailComponent);
        verifyAltEmailComponent = verifyAltEmailComponentFixture.componentInstance;
        verifyAltEmailDebugElement = verifyAltEmailComponentFixture.debugElement;
        stubActivatedRoute = TestBed.get(ActivatedRoute) as StubActivatedRoute;
    });

    describe(`ngOnInit`, () => {
        it(`ngOnInit sets linkExpiredOrInvalid to true if responseModel.invalidToken is true`, () => {
            let responseModel: SetAltEmailVerifiedResponseModel = { invalidToken: true };
            stubActivatedRoute.testData = { responseModel: responseModel };

            verifyAltEmailComponentFixture.detectChanges();

            expect(verifyAltEmailComponent.linkExpiredOrInvalid).toBe(true);
        });

        it(`ngOnInit does not set linkExpiredOrInvalid to true if responseModel.invalidToken is false`, () => {
            let responseModel: SetAltEmailVerifiedResponseModel = { invalidToken: false };
            stubActivatedRoute.testData = { responseModel: responseModel };

            verifyAltEmailComponentFixture.detectChanges();

            expect(verifyAltEmailComponent.linkExpiredOrInvalid).toBe(false);
        });
    });

    it(`Renders link expired or invalid tip if linkExpiredOrInvalid is true`, () => {
        let responseModel: SetAltEmailVerifiedResponseModel = { invalidToken: true };
        stubActivatedRoute.testData = { responseModel: responseModel };
        verifyAltEmailComponentFixture.detectChanges();

        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(verifyAltEmailDebugElement, `Link expired or invalid`)).toBe(true);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(verifyAltEmailDebugElement, `Get a new link`)).toBe(true);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(verifyAltEmailDebugElement, `Your alternative email has been verified`)).toBe(false);
    });

    it(`Renders alternative email has been verified tip if linkExpiredOrInvalid is false`, () => {
        let responseModel: SetAltEmailVerifiedResponseModel = { invalidToken: false };
        stubActivatedRoute.testData = { responseModel: responseModel };
        verifyAltEmailComponentFixture.detectChanges();

        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(verifyAltEmailDebugElement, `Your alternative email has been verified`)).toBe(true);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(verifyAltEmailDebugElement, `Link expired or invalid`)).toBe(false);
        expect(DebugElementExtensions.
            hasDescendantWithInnerHtml(verifyAltEmailDebugElement, `Get a new link`)).toBe(false);
    });
});
