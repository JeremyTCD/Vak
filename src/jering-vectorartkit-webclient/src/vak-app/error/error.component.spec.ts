/* tslint:disable:no-unused-variable */
import { DebugElement, Component, Input, ViewChild } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';

import { ErrorComponent } from './error.component';
import { StubActivatedRoute } from '../../testing/router-stubs';

let errorComponent: ErrorComponent;
let errorComponentFixture: ComponentFixture<ErrorComponent>;
let errorComponentDebugElement: DebugElement;
let stubActivatedRoute: StubActivatedRoute;

let testErrorMessage = `testErrorMessage`;

describe('ErrorComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [ErrorComponent],
            providers: [{ provide: ActivatedRoute, useClass: StubActivatedRoute }]
        }).compileComponents();
    }));

    beforeEach(() => {
        errorComponentFixture = TestBed.createComponent(ErrorComponent);
        errorComponent = errorComponentFixture.componentInstance;
        errorComponentDebugElement = errorComponentFixture.debugElement;
        stubActivatedRoute = TestBed.get(ActivatedRoute) as StubActivatedRoute;
    });

    it(`Subscribes to params and does not render error message if it is null`, () => {
        let errorDebugElement: DebugElement;

        stubActivatedRoute.testParams = { errorMessage: `` };
        errorComponentFixture.detectChanges();
        errorDebugElement = errorComponentDebugElement.query(By.css(`div`));
        expect(errorDebugElement).toBeNull();

        stubActivatedRoute.testParams = { };
        errorComponentFixture.detectChanges();
        errorDebugElement = errorComponentDebugElement.query(By.css(`div`));
        expect(errorDebugElement).toBeNull();

        stubActivatedRoute.testParams = { errorMessage: null};
        errorComponentFixture.detectChanges();
        errorDebugElement = errorComponentDebugElement.query(By.css(`div`));
        expect(errorDebugElement).toBeNull();
    });

    it(`Subscribes to params and renders error message if it is a non-empty string`, () => {
        stubActivatedRoute.testParams = { errorMessage: testErrorMessage };
        errorComponentFixture.detectChanges();
        let errorNativeElement = errorComponentDebugElement.query(By.css(`div`)).nativeElement;
        expect(errorNativeElement.textContent).toBe(testErrorMessage);
    });
});
