import { DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router/index';
import { Observable } from 'rxjs';

import { ManageAccountComponent } from './manage-account.component';
import { StubActivatedRoute, StubRouter } from '../../testing/router-stubs';
import { GetAccountDetailsResponseModel } from '../shared/response-models/get-account-details.response-model';
import { HttpService } from '../shared/http.service';
import { StubHttpService } from '../../testing/http.service.stub';

let testUsername = `testUsername`;
let testReturnUrl = `testReturnUrl`;
let testEmail = `testEmail`;
let testDisplayName = `testDisplayName`;
let manageAccountComponentFixture: ComponentFixture<ManageAccountComponent>;
let manageAccountComponent: ManageAccountComponent;
let manageAccountDebugElement: DebugElement;
let testAccountDetailsResponseModel: GetAccountDetailsResponseModel;
let stubActivatedRoute: StubActivatedRoute;
let stubHttpService: StubHttpService;
let stubRouter: StubRouter;

describe('ManageAccountComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ManageAccountComponent],
            providers: [{ provide: ActivatedRoute, useClass: StubActivatedRoute },
                { provide: HttpService, useClass: StubHttpService },
                { provide: Router, useClass: StubRouter }]
        }).compileComponents();
    }));

    beforeEach(() => {
        manageAccountComponentFixture = TestBed.createComponent(ManageAccountComponent);
        manageAccountComponent = manageAccountComponentFixture.componentInstance;
        manageAccountDebugElement = manageAccountComponentFixture.debugElement;
        testAccountDetailsResponseModel = {};
        stubActivatedRoute = TestBed.get(ActivatedRoute) as StubActivatedRoute;
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        stubHttpService = TestBed.get(HttpService) as StubHttpService;
        stubRouter = TestBed.get(Router) as StubRouter;
    });

    it(`ngOnInit sets accountDetails`, () => {
        manageAccountComponentFixture.detectChanges();

        expect(manageAccountComponent.accountDetails).toBe(testAccountDetailsResponseModel);
    });

    describe(`Binds setTwoFactorEnabled`, () => {
        it(`Calls setTwoFactorEnabled(true) if twoFactorEnabled is false`, () => {
            testAccountDetailsResponseModel = { twoFactorEnabled: false }
            stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
            manageAccountComponentFixture.detectChanges();

            spyOn(manageAccountComponent, `setTwoFactorEnabled`);
            let anchor = manageAccountDebugElement.
                queryAll(By.css(`a`)).
                find(de => de.nativeElement.textContent.trim() === `Enable two factor auth`);

            anchor.triggerEventHandler(`click`, null);

            expect(manageAccountComponent.setTwoFactorEnabled).toHaveBeenCalledWith(true);
        });

        it(`Calls setTwoFactorEnabled(false) if twoFactorEnabled is true`, () => {
            testAccountDetailsResponseModel = { twoFactorEnabled: true }
            stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
            manageAccountComponentFixture.detectChanges();

            spyOn(manageAccountComponent, `setTwoFactorEnabled`);
            let anchor = manageAccountDebugElement.
                queryAll(By.css(`a`)).
                find(de => de.nativeElement.textContent.trim() === `Disable two factor auth`);

            anchor.triggerEventHandler(`click`, null);

            expect(manageAccountComponent.setTwoFactorEnabled).toHaveBeenCalledWith(false);
        });
    });

    it(`Binds sendEmailVerificationEmail`, () => {
        testAccountDetailsResponseModel = { emailVerified: false }
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponent.emailVerificationEmailSent = false;
        manageAccountComponentFixture.detectChanges();

        spyOn(manageAccountComponent, `sendEmailVerificationEmail`);
        let anchor = manageAccountDebugElement.
            queryAll(By.css(`a`)).
            find(de => de.nativeElement.textContent.trim() === `Send verification email`);

        anchor.triggerEventHandler(`click`, null);

        expect(manageAccountComponent.sendEmailVerificationEmail).toHaveBeenCalledTimes(1);
    });

    it(`Binds sendAltEmailVerificationEmail`, () => {
        testAccountDetailsResponseModel = {
            altEmail: testEmail,
            altEmailVerified: false,
            emailVerified: true // hide send verification email link for email
        }
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponent.altEmailVerificationEmailSent = false;
        manageAccountComponentFixture.detectChanges();

        spyOn(manageAccountComponent, `sendAltEmailVerificationEmail`);
        let anchor = manageAccountDebugElement.
            queryAll(By.css(`a`)).
            find(de => de.nativeElement.textContent.trim() === `Send verification email`);

        anchor.triggerEventHandler(`click`, null);

        expect(manageAccountComponent.sendAltEmailVerificationEmail).toHaveBeenCalledTimes(1);
    });

    it(`sendEmailVerificationEmail calls HttpService.post and sets emailVerificationEmailSent to true`, () => {
        spyOn(stubHttpService, `post`).and.returnValue(Observable.of(null));

        manageAccountComponent.sendEmailVerificationEmail();

        expect(stubHttpService.post).toHaveBeenCalledWith(`Account/SendEmailVerificationEmail`, null);
        expect(manageAccountComponent.emailVerificationEmailSent).toBe(true);
    });

    it(`sendAltEmailVerificationEmail calls HttpService.post and sets altEmailVerificationEmailSent to true`, () => {
        spyOn(stubHttpService, `post`).and.returnValue(Observable.of(null));

        manageAccountComponent.sendAltEmailVerificationEmail();

        expect(stubHttpService.post).toHaveBeenCalledWith(`Account/SendAltEmailVerificationEmail`, null);
        expect(manageAccountComponent.altEmailVerificationEmailSent).toBe(true);
    });

    describe(`setTwoFactorEnabled`, () => {
        it(`setTwoFactorEnabled calls HttpService.post and sets accountDetails.twoFactorEnabled to enabled if request succeeds`, () => {
            manageAccountComponent.accountDetails = {};
            spyOn(stubHttpService, `post`).and.returnValue(Observable.of(null));

            manageAccountComponent.setTwoFactorEnabled(true);

            expect(stubHttpService.post).toHaveBeenCalledWith(`Account/SetTwoFactorEnabled`, { enabled: true });
            expect(manageAccountComponent.accountDetails.twoFactorEnabled).toBe(true);
        });

        it(`setTwoFactorEnabled calls HttpService.post and navigates to two-factor-verify-email if request fails`, () => {
            manageAccountComponent.accountDetails = {};
            spyOn(stubHttpService, `post`).and.returnValue(Observable.throw(null));
            spyOn(stubRouter, `navigate`);

            manageAccountComponent.setTwoFactorEnabled(true);

            expect(stubRouter.navigate).toHaveBeenCalledWith([`two-factor-verify-email`]);
        });
    });

    it(`Renders email address verified tip if accountDetails.emailVerified is true`, () => {
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

    describe(`If accountDetails.emailVerified is false`, () => {
        it(`Renders email address unverified tip`, () => {
            testAccountDetailsResponseModel = { emailVerified: false }
            stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
            manageAccountComponentFixture.detectChanges();

            let divs = manageAccountDebugElement.queryAll(By.css(`div`));
            expect(divs.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Email address verified`)).toBe(false);
            expect(divs.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Email address unverified`)).toBe(true);
        });

        it(`Renders send verification email link if emailVerificationEmailSent is false`, () => {
            testAccountDetailsResponseModel = { emailVerified: false }
            stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
            manageAccountComponentFixture.detectChanges();

            let divs = manageAccountDebugElement.queryAll(By.css(`div`));
            expect(divs.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Email verification email sent.`)).toBe(false);
            let anchors = manageAccountDebugElement.queryAll(By.css(`a`));
            expect(anchors.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Did not receive email help`)).toBe(false);
            expect(manageAccountDebugElement.queryAll(By.css(`a`)).
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Send verification email`)).toBe(true);
        });

        it(`Renders email verification email sent tip if emailVerificationEmailSent is true`, () => {
            testAccountDetailsResponseModel = { emailVerified: false }
            manageAccountComponent.emailVerificationEmailSent = true;
            stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
            manageAccountComponentFixture.detectChanges();

            let divs = manageAccountDebugElement.queryAll(By.css(`div`));
            expect(divs.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Email verification email sent.`)).toBe(true);
            let anchors = manageAccountDebugElement.queryAll(By.css(`a`));
            expect(anchors.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Did not receive email help`)).toBe(true);
            expect(manageAccountDebugElement.queryAll(By.css(`a`)).
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Send verification email`)).toBe(false);
        });
    });

    it(`Renders alt email address not set tip if accountDetails.altEmail is falsey`, () => {
        testAccountDetailsResponseModel = { altEmail: undefined }
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponentFixture.detectChanges();

        let divs = manageAccountDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Alternative email address not set`)).toBe(true);
    });

    describe(`If accountDetails.altEmail is truthy`, () => {
        it(`Renders alt email address`, () => {
            testAccountDetailsResponseModel = { altEmail: testEmail }
            stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
            manageAccountComponentFixture.detectChanges();

            let divs = manageAccountDebugElement.queryAll(By.css(`div`));
            expect(divs.
                some(debugElement => debugElement.nativeElement.textContent.trim() === `Alternative email address: ${testEmail}`)).toBe(true);
        });

        it(`Renders alterntive email address verified tip if accountDetails.altEmailVerified is true`, () => {
            testAccountDetailsResponseModel = { altEmail: testEmail, altEmailVerified: true, emailVerified: true };
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

        describe(`If accountDetails.altEmailVerified is false`, () => {
            it(`Renders alt email address unverified tip`, () => {
                testAccountDetailsResponseModel = { altEmail: testEmail, altEmailVerified: false };
                stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
                manageAccountComponentFixture.detectChanges();

                let divs = manageAccountDebugElement.queryAll(By.css(`div`));
                expect(divs.
                    some(debugElement => debugElement.nativeElement.textContent.trim() === `Alternative email address verified`)).toBe(false);
                expect(divs.
                    some(debugElement => debugElement.nativeElement.textContent.trim() === `Alternative email address unverified`)).toBe(true);
            });

            it(`Renders send verification email link if altEmailVericiationEmailSent is false`, () => {
                testAccountDetailsResponseModel = { altEmail: testEmail, altEmailVerified: false };
                manageAccountComponent.altEmailVerificationEmailSent = false;
                stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
                manageAccountComponentFixture.detectChanges();

                let divs = manageAccountDebugElement.queryAll(By.css(`div`));
                expect(divs.
                    some(debugElement => debugElement.nativeElement.textContent.trim() === `Email verification email sent.`)).toBe(false);
                let anchors = manageAccountDebugElement.queryAll(By.css(`a`));
                expect(anchors.
                    some(debugElement => debugElement.nativeElement.textContent.trim() === `Did not receive email help`)).toBe(false);
                expect(manageAccountDebugElement.queryAll(By.css(`a`)).
                    filter(debugElement => debugElement.nativeElement.textContent.trim() === `Send verification email`).
                    length).toBe(2);
            });

            it(`Renders alt email verification email sent tip if altEmailVerificationEmailSent is true`, () => {
                testAccountDetailsResponseModel = { altEmail: testEmail, altEmailVerified: false }
                manageAccountComponent.altEmailVerificationEmailSent = true;
                stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
                manageAccountComponentFixture.detectChanges();

                let divs = manageAccountDebugElement.queryAll(By.css(`div`));
                expect(divs.
                    some(debugElement => debugElement.nativeElement.textContent.trim() === `Email verification email sent.`)).toBe(true);
                let anchors = manageAccountDebugElement.queryAll(By.css(`a`));
                expect(anchors.
                    some(debugElement => debugElement.nativeElement.textContent.trim() === `Did not receive email help`)).toBe(true);
                // Send verification email for email still visible
                expect(manageAccountDebugElement.queryAll(By.css(`a`)).
                    filter(debugElement => debugElement.nativeElement.textContent.trim() === `Send verification email`).
                    length).toBe(1);
            });
        });
    });

    it(`Renders display name if accountDetails.displayName is truthy`, () => {
        testAccountDetailsResponseModel = { displayName: testDisplayName };
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponentFixture.detectChanges();

        let divs = manageAccountDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Display name: ${testDisplayName}`)).toBe(true);
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Display name not set`)).toBe(false);
    });

    it(`Renders display name not set tip if accountDetails.displayName is falsey`, () => {
        testAccountDetailsResponseModel = { displayName: undefined }
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponentFixture.detectChanges();

        let divs = manageAccountDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Display name not set`)).toBe(true);
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Display name: ${testDisplayName}`)).toBe(false);
    });

    it(`Renders two factor auth disabled tip and enable two factor auth link if accountDetails.twoFactorEnabled is false`, () => {
        testAccountDetailsResponseModel = { twoFactorEnabled: false };
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponentFixture.detectChanges();

        let divs = manageAccountDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Two factor auth disabled`)).toBe(true);
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Two factor auth enabled`)).toBe(false);
        let anchors = manageAccountDebugElement.queryAll(By.css(`a`));
        expect(anchors.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Enable two factor auth`)).toBe(true);
        expect(anchors.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Disable two factor auth`)).toBe(false);
    });

    it(`Renders two factor auth enabled tip and disable two factor auth link if accountDetails.twoFactorEnabled is true`, () => {
        testAccountDetailsResponseModel = { twoFactorEnabled: true };
        stubActivatedRoute.testData = { responseModel: testAccountDetailsResponseModel };
        manageAccountComponentFixture.detectChanges();

        let divs = manageAccountDebugElement.queryAll(By.css(`div`));
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Two factor auth enabled`)).toBe(true);
        expect(divs.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Two factor auth disabled`)).toBe(false);
        let anchors = manageAccountDebugElement.queryAll(By.css(`a`));
        expect(anchors.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Disable two factor auth`)).toBe(true);
        expect(anchors.
            some(debugElement => debugElement.nativeElement.textContent.trim() === `Enable two factor auth`)).toBe(false);
    });
});

