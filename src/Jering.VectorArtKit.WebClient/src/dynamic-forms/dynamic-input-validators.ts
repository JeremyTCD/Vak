import { FormControl } from '@angular/forms';
import { isBlank, isString, isValue, isEmptyString, isDigit } from '../utility/type-check';
import { DynamicInputValidatorData } from './dynamic-input-validator-data';
import { DynamicInput } from './dynamic-input-base';

/**
 * Provides a set of validators used by form controls.
 *
 * A validator is a function that processes a {@link FormControl} or collection of
 * controls and returns a map of errors. A null map means that validation has passed.
 *
 * ### Example
 *
 * ```typescript
 * var loginControl = new FormControl("", Validators.required)
 * ```
 *
 * @stable
 */
export class DynamicInputValidators {
    /**
     * Validator that requires controls to have a non-empty value.
     */
    static validateRequired(validatorData: DynamicInputValidatorData, dynamicInput: DynamicInput<any>): DynamicInputValidator {
        return (control: FormControl): { [key: string]: string } => {
            return isValue(control.value) ?
                null :
                { 'validateRequired': validatorData.errorMessage.replace('{0}', dynamicInput.displayName) };
        }
    }

    /**
     * Validator that requires control's value to be composed of only digits. 
     */
    static validateAllDigits(validatorData: DynamicInputValidatorData, dynamicInput: DynamicInput<any>): DynamicInputValidator {
        return (control: FormControl): { [key: string]: string } => {
            if (!isString(control.value) || isEmptyString(control.value))
                return null;

            for (let char of control.value) {
                if (!isDigit(char)) {
                    return { 'validateAllDigits': validatorData.errorMessage }
                }
            }

            return null;
        }
    }

    /**
     * Validator that requires control's value to be a valid email address. 
     */
    static validateEmailAddress(validatorData: DynamicInputValidatorData, dynamicInput: DynamicInput<any>): DynamicInputValidator {
        return (control: FormControl): { [key: string]: string } => {
            if (!isString(control.value) || isEmptyString(control.value))
                return null;

            // Email is valid if there is only 1 '@' character
            // and it is neither the first nor the last character
            let value: string = control.value;
            let found = false;
            for (let i = 0; i < value.length; i++) {
                if (value[i] == '@') {
                    if (found || i == 0 || i == value.length - 1) {
                        return { 'validateEmailAddress': validatorData.errorMessage };
                    }
                    found = true;
                }
            }

            return found ? null : { 'validateEmailAddress': validatorData.errorMessage };
        }
    }

    
}

export interface DynamicInputValidator { (control: FormControl): { [key: string]: any }; }