import { Check } from '../../utility/check';
import { DynamicInputValidatorData } from './dynamic-input-validator-data';
import { DynamicInput } from './dynamic-input';

/**
 * Provides a set of DynamicInputValidator functions used by FormControls.
 *
 * A DynamicInputValidator is a function that processes a FormControl or collection of
 * controls and returns a map of errors. A null map means that validation has passed.
 */
export class DynamicInputValidators {
    /**
     * Returns a DynamicInputValidator function.
     *
     * Generated DynamicInputValidator function returns
     * - null if control value contains only digits
     * - null if control value is null, undefined or an empty string
     * - error message if control value does not contain only digits
     */
    static validateAllDigits(validatorData: DynamicInputValidatorData): DynamicInputValidator {
        return (dynamicInput: DynamicInput<any>): string => {
            if (!Check.isValue(dynamicInput.value)) {
                return null;
            }

            for (let char of dynamicInput.value) {
                if (!Check.isDigit(char)) {
                    return validatorData.errorMessage;
                }
            }

            return null;
        };
    }

    /**
     * Returns a DynamicInputValidator function.
     *
     * Generated DynamicInputValidator function returns
     * - null if control value is sufficiently complex
     * - null if control value is null, undefined or an empty string
     * - error message if control value is not sufficiently complex
     */
    static validateComplexity(validatorData: DynamicInputValidatorData): DynamicInputValidator {
        return (dynamicInput: DynamicInput<any>): string => {
            if (!Check.isValue(dynamicInput.value)) {
                return null;
            }

            let numPossibilitiesPerChar = 0;
            let hasLower = false, hasUpper = false, hasDigit = false, hasNonAlphanumeric = false;

            for (let char of dynamicInput.value) {
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

            if (Math.pow(numPossibilitiesPerChar, dynamicInput.value.length) < 2.8E12) {
                return validatorData.errorMessage;
            }

            return null;
        };
    }

    /**
     * Returns a DynamicInputValidator function.
     *
     * Generated DynamicInputValidator function returns
     * - null if either control value or other value is null, undefined or an empty string
     * - null if control values differ
     * - error message if control values do not differ
     */
    static validateDiffers(validatorData: DynamicInputValidatorData): DynamicInputValidator {
        let otherControlName = validatorData.options['OtherProperty'];

        return (dynamicInput: DynamicInput<any>): string => {
            let otherValue = dynamicInput.parent.get(otherControlName).value;

            if (!Check.isValue(dynamicInput.value) || !Check.isValue(otherValue) ||
                dynamicInput.value !== otherValue) {
                return null;
            }

            return validatorData.errorMessage;
        };
    }

    /**
     * Returns a DynamicInputValidator function.
     *
     * Generated DynamicInputValidator function returns
     * - null if control value is an email address
     * - null if control value is null, undefined or an empty string
     * - error message if control value is not an email address
     */
    static validateEmailAddress(validatorData: DynamicInputValidatorData): DynamicInputValidator {
        return (dynamicInput: DynamicInput<any>): string => {
            if (!Check.isValue(dynamicInput.value)) {
                return null;
            }

            // Email address is valid if there is only 1 '@' character
            // and it is neither the first nor the last character
            let value: string = dynamicInput.value;
            let found = false;
            for (let i = 0; i < value.length; i++) {
                if (value[i] === '@') {
                    if (found || i === 0 || i === value.length - 1) {
                        return validatorData.errorMessage;
                    }
                    found = true;
                }
            }

            return found ? null : validatorData.errorMessage;
        };
    }

    /**
     * Returns a DynamicInputValidator function.
     *
     * Generated DynamicInputValidator function returns
     * - null if control value is has specified length
     * - null if control value is null, undefined or an empty string
     * - error message if control value does not have specified length
     */
    static validateLength(validatorData: DynamicInputValidatorData): DynamicInputValidator {
        let length = parseInt(validatorData.options['Length'], 10);

        return (dynamicInput: DynamicInput<any>): string => {
            if (!Check.isValue(dynamicInput.value) || dynamicInput.value.length === length) {
                return null;
            }

            return validatorData.errorMessage;
        };
    }

    /**
     * Returns a DynamicInputValidator function.
     *
     * Generated DynamicInputValidator function returns
     * - null if either control value is null, undefined or an empty string
     * - null if control values match
     * - error message if control values do not match
     */
    static validateMatches(validatorData: DynamicInputValidatorData): DynamicInputValidator {
        let otherControlName = validatorData.options['OtherProperty'];

        return (dynamicInput: DynamicInput<any>): string => {
            let otherValue = dynamicInput.parent.get(otherControlName).value;

            if (!Check.isValue(dynamicInput.value) || !Check.isValue(otherValue) || dynamicInput.value === otherValue) {
                return null;
            }

            return validatorData.errorMessage;
        };
    }

    /**
     * Returns a DynamicInputValidator function.
     *
     * Generated DynamicInputValidator function returns
     * - null if control value's length is greater than or equal to specified minimum length
     * - null if control value is null, undefined or an empty string
     * - error message if control value's length is less than specified minimum length length
     */
    static validateMinLength(validatorData: DynamicInputValidatorData): DynamicInputValidator {
        let length = parseInt(validatorData.options['MinLength'], 10);

        return (dynamicInput: DynamicInput<any>): string => {
            if (!Check.isValue(dynamicInput.value) || dynamicInput.value.length >= length) {
                return null;
            }

            return validatorData.errorMessage;
        };
    }

    /**
     * Returns a DynamicInputValidator function.
     *
     * Generated DynamicInputValidator function returns
     * - null if control value is not null, undefined or an empty string.
     * - error message if control value is null, undefined or an empty string.
     */
    static validateRequired(validatorData: DynamicInputValidatorData): DynamicInputValidator {
        return (dynamicInput: DynamicInput<any>): string => {
            return Check.isValue(dynamicInput.value) ?
                null :
                validatorData.errorMessage.replace('{0}', dynamicInput.displayName);
        };
    }
}

export interface DynamicInputValidator { (dynamicInput: DynamicInput<any>): string; }