import { Subscription, Subject, Observable, Observer } from 'rxjs';

import { DynamicFormsService } from '../dynamic-forms.service';
import { DynamicControlValidator } from './dynamic-control-validator';
import { ValidatorResponseModel } from '../response-models/validator.response-model';
import { DynamicControl } from './dynamic-control';
import { Validity } from '../validity';
import { DynamicControlValidatorResult } from './dynamic-control-validator-result';
import { Check } from '../../check';

export class DynamicControlAsyncValidator {
    subject = new Subject<string>();
    subjectAsObservable: Observable<Validity>;
    observer: Observer<Validity>;
    subscription: Subscription;

    /**
     * Constructor
     *
     * Instantiates subjectAsObservable, observer and subscription.
     *
     * observer
     * - Directly sets dynamicControl.validity to Validity.valid if query observable returns Validity.valid
     * - Otherwise, directly sets dynamicControl.validity to Validity.invalid
     */
    constructor(validatorResponseModel: ValidatorResponseModel, dynamicControl: DynamicControl<any>,
        dynamicFormsService: DynamicFormsService) {

        this.subjectAsObservable = this.subject.
            debounceTime(200).
            map((value: string) => {
                return dynamicFormsService.
                    validateValue(validatorResponseModel.options[`RelativeUrl`], value);
            }).
            switch();

        this.observer = {
            next: validity => {
                if (validity === Validity.valid) {
                    dynamicControl.validity = Validity.valid;
                } else {
                    dynamicControl.validity = Validity.invalid;
                    dynamicControl.messages.push(validatorResponseModel.errorMessage);
                }
                dynamicControl.tryValidateParent();
            },
            error: error => {
                dynamicControl.validity = Validity.valid;
            },
            complete: () => {
                // Do nothing
            }
        }

        this.subscription = this.subjectAsObservable.subscribe(this.observer);
    }

    /**
     * Unsubscribes from subscription is invalid before function is called or if dynamicControl.value
     * is not a value. Otherwise, subscribes to subjectAsObservable if subscription is closed and calls
     * subject.next.
     * 
     * Returns
     * - DynamicControlValidatorResult with validity = Validity.pending and message = undefined if
     *   dynamicControl.validity is valid before function is called
     * - DynamicControlValidatorResult with validity = Validity.invalid and message = undefined if
     *   dynamicControl.validity is invalid before function is called
     * - DynamicControlValidatorResult with validity = Validity.valid and message = undefined if
     *   control value is null, undefined or an empty string
     */
    validate(dynamicControl: DynamicControl<any>): DynamicControlValidatorResult {
        if (dynamicControl.validity === Validity.invalid) {
            this.subscription.unsubscribe();

            return new DynamicControlValidatorResult(Validity.invalid);
        } else if (!Check.isValue(dynamicControl.value)) {
            this.subscription.unsubscribe();

            return new DynamicControlValidatorResult(Validity.valid);
        } else {
            if (this.subscription.closed) {
                this.subscription = this.subjectAsObservable.subscribe(this.observer);
            }
            this.subject.next(dynamicControl.value);

            return new DynamicControlValidatorResult(Validity.pending);
        }
    }
}

