import { Injectable } from '@angular/core';
import { ReplaySubject, BehaviorSubject, Observable } from 'rxjs';

@Injectable()
export class StubActivatedRoute {

    // BehaviourSubject stores the last emitted value and emits it immediately to new subscribers
    private _subject = new BehaviorSubject(this.testParams);
    public params: Observable<any> = this._subject.asObservable();

    // Test parameters
    private _testParams: {};
    get testParams() { return this._testParams; }
    set testParams(params: {}) {
        this._testParams = params;
        this._subject.next(params);
    }

    private _data = new ReplaySubject(1);
    public data: Observable<any> = this._data.asObservable();
    private _testData: {};
    get testData() { return this._testData; }
    set testData(data: {}) {
        this._testData = data;
        this._data.next(data);
    }

    // ActivatedRoute.snapshot
    get snapshot() {
        return { params: this.testParams, data: this.testData }; 
    }
}

export class StubRouter {
    navigate(commands: any[]): void {
    }
}