import { DynamicControlValidator } from './dynamic-control-validator';
import { Subscription } from 'rxjs';

/**
 * Data used to defined a DynamicControlValidator
 */
export class DynamicControlAsyncValidator {
    constructor(public validate: DynamicControlValidator, private _subscription: Subscription) {
    }

    unsubscribe(): void {
        this._subscription.unsubscribe();
    }
}

