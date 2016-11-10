import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Injectable()
export class StubActivatedRoute {

    // BehaviourSubject stores the last emitted value and emits it immediately to new subscribers
    private _subject = new BehaviorSubject(this.testParams);
    public params = this._subject.asObservable();

    // Test parameters
    private _testParams: {};
    get testParams() { return this._testParams; }
    set testParams(params: {}) {
        this._testParams = params;
        this._subject.next(params);
    }

    // ActivatedRoute.snapshot.params
    get snapshot() {
        return { params: this.testParams };
    }
}

export class StubRouter {
    navigate(commands: any[]): void {
    }
}