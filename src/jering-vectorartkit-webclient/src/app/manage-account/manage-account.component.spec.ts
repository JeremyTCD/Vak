import { DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router/index';

import { ManageAccountComponent } from './manage-account.component';
import { StubActivatedRoute } from '../../testing/router-stubs';
import { GetAccountDetailsResponseModel } from '../shared/response-models/get-account-details.response-model';

let testUsername = `testUsername`;
let testReturnUrl = `testReturnUrl`;
let testEmail = `testEmail`;
let testDisplayName = `testDisplayName`;
let manageAccountComponentFixture: ComponentFixture<ManageAccountComponent>;
let manageAccountComponent: ManageAccountComponent;
let manageAccountDebugElement: DebugElement;
let testAccountDetailsResponseModel: GetAccountDetailsResponseModel;
let stubActivatedRoute: StubActivatedRoute;

describe('ManageAccountComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ManageAccountComponent],
            providers: [{ provide: ActivatedRoute, useClass: StubActivatedRoute }]
        }).compileComponents();
    }));

    beforeEach(() => {
        manageAccountComponentFixture = TestBed.createComponent(ManageAccountComponent);
        manageAccountComponent = manageAccountComponentFixture.componentInstance;
        manageAccountDebugElement = manageAccountComponentFixture.debugElement;
        testAccountDetailsResponseModel = {};
        stubActivatedRoute = TestBed.get(ActivatedRoute) as StubActivatedRoute;
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
    });

    it(`ngOnInit sets responseModel`, () => {
        manageAccountComponentFixture.detectChanges();

        expect(manageAccountComponent.responseModel).toBe(testAccountDetailsResponseModel);
    });

    it(`Renders email address verified tip if responseModel.emailVerified is true`, () => {
        testAccountDetailsResponseModel = { emailVerified: true }
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponentFixture.detectChanges();

        let divs = manageAccountDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Email address verified`)).toBe(true);
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Email address unverified`)).toBe(false);
        expect(manageAccountDebugElement.queryAll(By.css(`a`)).
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Send verification email`)).toBe(false);
    });

    it(`Renders email address unverified tip and send verification email link if responseModel.emailVerified is false`, () => {
        testAccountDetailsResponseModel = { emailVerified: false }
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponentFixture.detectChanges();

        let divs = manageAccountDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Email address verified`)).toBe(false);
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Email address unverified`)).toBe(true);
        expect(manageAccountDebugElement.queryAll(By.css(`a`)).
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Send verification email`)).toBe(true);
    });

    it(`Renders alternative email address not set tip if responseModel.alternativeEmail is falsey`, () => {
        testAccountDetailsResponseModel = { alternativeEmail: undefined }
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponentFixture.detectChanges();

        let divs = manageAccountDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Alternative email address not set`)).toBe(true);
    });

    describe(`If responseModel.alternativeEmail is truthy`, () => {
        it(`Renders alternative email address`, () => {
            testAccountDetailsResponseModel = { alternativeEmail: testEmail }
            stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
            manageAccountComponentFixture.detectChanges();

            let divs = manageAccountDebugElement.queryAll(By.css(`div`));
            expect(divs.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Alternative email address: ${testEmail}`)).toBe(true);
        });

        it(`Renders alterntive email address verified tip if responseModel.alternativeEmailVerified is true`, () => {
            testAccountDetailsResponseModel = { alternativeEmail: testEmail, alternativeEmailVerified: true, emailVerified: true }
            stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
            manageAccountComponentFixture.detectChanges();

            let divs = manageAccountDebugElement.queryAll(By.css(`div`));
            expect(divs.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Alternative email address verified`)).toBe(true);
            expect(divs.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Alternative email address unverified`)).toBe(false);
            expect(manageAccountDebugElement.queryAll(By.css(`a`)).
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Send verification email`)).toBe(false);
        });

        it(`Renders alternative email address unverified tip and send verification email link if responseModel.alternativeEmailVerified is false`, () => {
            testAccountDetailsResponseModel = { alternativeEmail: testEmail, alternativeEmailVerified: false }
            stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
            manageAccountComponentFixture.detectChanges();

            let divs = manageAccountDebugElement.queryAll(By.css(`div`));
            expect(divs.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Alternative email address verified`)).toBe(false);
            expect(divs.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Alternative email address unverified`)).toBe(true);
            expect(manageAccountDebugElement.queryAll(By.css(`a`)).
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Send verification email`)).toBe(true);
        });
    });

    it(`Renders display name if responseModel.displayName is truthy`, () => {
        testAccountDetailsResponseModel = { displayName: testDisplayName };
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponentFixture.detectChanges();

        let divs = manageAccountDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Display name: ${testDisplayName}`)).toBe(true);
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Display name not set`)).toBe(false);
    });

    it(`Renders display name not set tip if responseModel.displayName is falsey`, () => {
        testAccountDetailsResponseModel = { displayName: undefined }
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponentFixture.detectChanges();

        let divs = manageAccountDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Display name not set`)).toBe(true);
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Display name: ${testDisplayName}`)).toBe(false);
    });

    it(`Renders two factor authentication disabled tip and enable two factor authentication link if responseModel.twoFactorEnabled is false`, () => {
        testAccountDetailsResponseModel = { twoFactorEnabled: false };
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponentFixture.detectChanges();

        let divs = manageAccountDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Two factor authentication disabled`)).toBe(true);
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Two factor authentication enabled`)).toBe(false);
        let anchors = manageAccountDebugElement.queryAll(By.css(`a`));
        expect(anchors.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Enable two factor authentication`)).toBe(true);
        expect(anchors.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Disable two factor authentication`)).toBe(false);
    });

    it(`Renders two factor authentication enabled tip and disable two factor authentication link if responseModel.twoFactorEnabled is true`, () => {
        testAccountDetailsResponseModel = { twoFactorEnabled: true };
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponentFixture.detectChanges();

        let divs = manageAccountDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Two factor authentication enabled`)).toBe(true);
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Two factor authentication disabled`)).toBe(false);
        let anchors = manageAccountDebugElement.queryAll(By.css(`a`));
        expect(anchors.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Disable two factor authentication`)).toBe(true);
        expect(anchors.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Enable two factor authentication`)).toBe(false);
    });
});

