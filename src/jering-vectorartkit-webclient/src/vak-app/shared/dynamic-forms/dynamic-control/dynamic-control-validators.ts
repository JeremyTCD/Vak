import { Subject, Observable } from 'rxjs';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/switch';
import 'rxjs/add/operator/do';

import { environment } from '../../../../environments/environment';
import { Check } from '../../utility/check';
import { DynamicControlValidatorData } from './dynamic-control-validator-data';
import { DynamicControlValidator } from './dynamic-control-validator';
import { DynamicControl } from './dynamic-control';
import { DynamicFormsService } from '../dynamic-forms.service';
import { DynamicControlValidatorResult } from './dynamic-control-validator-result';
import { Validity } from '../validity';

/**
 * Provides a set of DynamicControlValidator functions used by FormControls.
 *
 * A DynamicControlValidator is a function that processes a FormControl or collection of
 * controls and returns a map of errors. A null map means that validation has passed.
 */
export class DynamicControlValidators {
    /**
     * Returns a DynamicControlValidator function.
     *
     * Generated DynamicControlValidator function returns
     * - null if control value contains only digits
     * - null if control value is null, undefined or an empty string
     * - error message if control value does not contain only digits
     */
    static validateAllDigits(validatorData: DynamicControlValidatorData, dynamicControl: DynamicControl<any>,
        dynamicFormsService: DynamicFormsService): DynamicControlValidator {
        return (dynamicControl: DynamicControl<any>): DynamicControlValidatorResult => {
            if (!Check.isValue(dynamicControl.value)) {
                return new DynamicControlValidatorResult(Validity.valid);
            }

            for (let char of dynamicControl.value) {
                if (!Check.isDigit(char)) {
                    return new DynamicControlValidatorResult(Validity.invalid, validatorData.errorMessage);
                }
            }

            return new DynamicControlValidatorResult(Validity.valid);
        };
    }

    /**
     * Returns a DynamicControlValidator function.
     *
     * Generated DynamicControlValidator function returns
     * - null if control value is sufficiently complex
     * - null if control value is null, undefined or an empty string
     * - error message if control value is not sufficiently complex
     */
    static validateComplexity(validatorData: DynamicControlValidatorData, dynamicControl: DynamicControl<any>,
        dynamicFormsService: DynamicFormsService): DynamicControlValidator {
        return (dynamicControl: DynamicControl<any>): DynamicControlValidatorResult => {
            if (!Check.isValue(dynamicControl.value)) {
                return new DynamicControlValidatorResult(Validity.valid);
            }

            let numPossibilitiesPerChar = 0;
            let hasLower = false, hasUpper = false, hasDigit = false, hasNonAlphanumeric = false;

            for (let char of dynamicControl.value) {
                if (!hasLower && Check.isLower(char)) {
                    numPossibilitiesPerChar += 26;
                    hasLower = true;
                }

                if (!hasUpper && Check.isUpper(char)) {
                    numPossibilitiesPerChar += 26;
                    hasUpper = true;
                }

                if (!hasDigit && Check.isDigit(char)) {
                    numPossibilitiesPerChar += 10;
                    hasDigit = true;
                }

                if (!hasNonAlphanumeric && Check.isNonAlphaNumeric(char)) {
                    numPossibilitiesPerChar += 32;
                    hasNonAlphanumeric = true;
                }
            }

            if (Math.pow(numPossibilitiesPerChar, dynamicControl.value.length) < 2.8E12) {
                return new DynamicControlValidatorResult(Validity.invalid, validatorData.errorMessage);
            }

            return new DynamicControlValidatorResult(Validity.valid);
        };
    }

    /**
     * Returns a DynamicControlValidator function.
     *
     * Generated DynamicControlValidator function returns
     * - null if either control value or other value is null, undefined or an empty string
     * - null if control values differ
     * - error message if control values do not differ
     */
    static validateDiffers(validatorData: DynamicControlValidatorData, dynamicControl: DynamicControl<any>,
        dynamicFormsService: DynamicFormsService): DynamicControlValidator {
        let otherControlName = validatorData.options['OtherProperty'];

        return (dynamicControl: DynamicControl<any>): DynamicControlValidatorResult => {
            let otherValue = dynamicControl.parent.get(otherControlName).value;

            if (!Check.isValue(dynamicControl.value) || !Check.isValue(otherValue) ||
                dynamicControl.value !== otherValue) {
                return new DynamicControlValidatorResult(Validity.valid);
            }

            return new DynamicControlValidatorResult(Validity.invalid, validatorData.errorMessage);
        };
    }

    /**
     * Returns a DynamicControlValidator function.
     *
     * Generated DynamicControlValidator function returns
     * - null if control value is an email address
     * - null if control value is null, undefined or an empty string
     * - error message if control value is not an email address
     */
    static validateEmailAddress(validatorData: DynamicControlValidatorData, dynamicControl: DynamicControl<any>,
        dynamicFormsService: DynamicFormsService): DynamicControlValidator {
        return (dynamicControl: DynamicControl<any>): DynamicControlValidatorResult => {
            if (!Check.isValue(dynamicControl.value)) {
                return new DynamicControlValidatorResult(Validity.valid);
            }

            // Email address is valid if there is only 1 '@' character
            // and it is neither the first nor the last character
            let value: string = dynamicControl.value;
            let found = false;
            for (let i = 0; i < value.length; i++) {
                if (value[i] === '@') {
                    if (found || i === 0 || i === value.length - 1) {
                        return new DynamicControlValidatorResult(Validity.invalid, validatorData.errorMessage);
                    }
                    found = true;
                }
            }

            return found ? new DynamicControlValidatorResult(Validity.valid) : new DynamicControlValidatorResult(Validity.invalid, validatorData.errorMessage);
        };
    }

    /**
     * Returns a DynamicControlValidator function.
     *
     * Generated DynamicControlValidator function returns
     * - null if control value is has specified length
     * - null if control value is null, undefined or an empty string
     * - error message if control value does not have specified length
     */
    static validateLength(validatorData: DynamicControlValidatorData, dynamicControl: DynamicControl<any>,
        dynamicFormsService: DynamicFormsService): DynamicControlValidator {
        let length = parseInt(validatorData.options['Length'], 10);

        return (dynamicControl: DynamicControl<any>): DynamicControlValidatorResult => {
            if (!Check.isValue(dynamicControl.value) || dynamicControl.value.length === length) {
                return new DynamicControlValidatorResult(Validity.valid);
            }

            return new DynamicControlValidatorResult(Validity.invalid, validatorData.errorMessage);
        };
    }

    /**
     * Returns a DynamicControlValidator function.
     *
     * Generated DynamicControlValidator function returns
     * - null if either control value is null, undefined or an empty string
     * - null if control values match
     * - error message if control values do not match
     */
    static validateMatches(validatorData: DynamicControlValidatorData, dynamicControl: DynamicControl<any>,
        dynamicFormsService: DynamicFormsService): DynamicControlValidator {
        let otherControlName = validatorData.options['OtherProperty'];

        return (dynamicControl: DynamicControl<any>): DynamicControlValidatorResult => {
            let otherValue = dynamicControl.parent.get(otherControlName).value;

            if (!Check.isValue(dynamicControl.value) || !Check.isValue(otherValue) || dynamicControl.value === otherValue) {
                return new DynamicControlValidatorResult(Validity.valid);
            }

            return new DynamicControlValidatorResult(Validity.invalid, validatorData.errorMessage);
        };
    }

    /**
     * Returns a DynamicControlValidator function.
     *
     * Generated DynamicControlValidator function returns
     * - null if control value's length is greater than or equal to specified minimum length
     * - null if control value is null, undefined or an empty string
     * - error message if control value's length is less than specified minimum length length
     */
    static validateMinLength(validatorData: DynamicControlValidatorData, dynamicControl: DynamicControl<any>,
        dynamicFormsService: DynamicFormsService): DynamicControlValidator {
        let length = parseInt(validatorData.options['MinLength'], 10);

        return (dynamicControl: DynamicControl<any>): DynamicControlValidatorResult => {
            if (!Check.isValue(dynamicControl.value) || dynamicControl.value.length >= length) {
                return new DynamicControlValidatorResult(Validity.valid);
            }

            return new DynamicControlValidatorResult(Validity.invalid, validatorData.errorMessage);
        };
    }

    /**
     * Returns a DynamicControlValidator function.
     *
     * Generated DynamicControlValidator function returns
     * - null if control value is not null, undefined or an empty string.
     * - error message if control value is null, undefined or an empty string.
     */
    static validateRequired(validatorData: DynamicControlValidatorData, dynamicControl: DynamicControl<any>,
        dynamicFormsService: DynamicFormsService): DynamicControlValidator {
        return (dynamicControl: DynamicControl<any>): DynamicControlValidatorResult => {
            return Check.isValue(dynamicControl.value) ?
                new DynamicControlValidatorResult(Validity.valid) :
                new DynamicControlValidatorResult(Validity.invalid, validatorData.errorMessage.replace('{0}', dynamicControl.displayName));
        };
    }

    /**
     * Returns a DynamicControlValidator function.
     *
     * Generated DynamicControlValidator function returns
     * - null if control value is not null, undefined or an empty string.
     * - error message if control value is null, undefined or an empty string.
     */
    static asyncValidateEmailNotInUse(validatorData: DynamicControlValidatorData, dynamicControl: DynamicControl<any>,
        dynamicFormsService: DynamicFormsService): DynamicControlValidator {
        let valueStream = new Subject<string>();
        valueStream.
            debounceTime(200).
            //distinctUntilChanged().
            //do((value: string) => {
            //    if (value !== null) {
            //        dynamicControl.validity = Validity.pending;
            //    }
            //}).
            map((value: string) => {
                if (value === null) {
                    return Observable.empty<Validity>();
                } 

                return dynamicFormsService.
                    validateValue(`${environment.apiUrl}${validatorData.options[`relativeUrl`]}`, value).
                    catch((error: any) =>
                        Observable.of(Validity.valid)
                    );
            }).
            switch().
            subscribe(
                validity => {
                    if (validity === Validity.valid) {
                        dynamicControl.validity = Validity.valid;
                    } else {
                        dynamicControl.validity = Validity.invalid;
                        dynamicControl.errors.push(validatorData.errorMessage);
                    }
                },
                error => {
                    dynamicControl.validity = Validity.valid;
                }
            );

        return (dynamicControl: DynamicControl<any>): DynamicControlValidatorResult => {
            if (dynamicControl.validity !== Validity.invalid) {
                valueStream.next(dynamicControl.value);

                return new DynamicControlValidatorResult(Validity.pending);
            } else {
                // This is necessary so that any pending validateValue Observables are unsubscribed from. See Observable.switchMap.
                valueStream.next(null);

                return new DynamicControlValidatorResult(Validity.invalid);
            }
        };
    }
}
