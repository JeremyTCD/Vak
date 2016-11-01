import { Observable } from 'rxjs';

import { DynamicControlValidators } from './dynamic-control-validators';
import { DynamicControlValidator } from './dynamic-control-validator';
import { DynamicControlAsyncValidator } from './dynamic-control-async-validator';
import { DynamicControlValidatorData } from './dynamic-control-validator-data';
import { DynamicControlValidatorResult } from './dynamic-control-validator-result';
import { DynamicControl } from './dynamic-control';
import { DynamicForm } from '../dynamic-form/dynamic-form';
import { DynamicFormsService } from '../dynamic-forms.service';
import { Validity } from '../validity';

let testErrorMessage = `testErrorMessage`;
let testDisplayName = `testDisplayName`;
let result: DynamicControlValidatorResult;

describe(`validateAllDigits`, () => {
    let dynamicControl: DynamicControl<any>;
    let validateAllDigitsValidator: DynamicControlValidator;

    beforeEach(() => {
        dynamicControl = new DynamicControl({});
        validateAllDigitsValidator = DynamicControlValidators.validateAllDigits({
            name: `validateAllDigits`,
            errorMessage: testErrorMessage,
            options: null
        });
    });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control value is null, undefined or an empty string`, () => {
            dynamicControl.value = null;
            result = validateAllDigitsValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = undefined;
            result = validateAllDigitsValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = ``;
            result = validateAllDigitsValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control value contains only digits`, () => {
            dynamicControl.value = `0`;
            result = validateAllDigitsValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = `12345`;
            result = validateAllDigitsValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message 
        if control value does not contain only digits`, () => {
            dynamicControl.value = `-1`;
            result = validateAllDigitsValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);

            dynamicControl.value = `1.1`;
            result = validateAllDigitsValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);

            dynamicControl.value = `a12345`;
            result = validateAllDigitsValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);

            dynamicControl.value = `test`;
            result = validateAllDigitsValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);
        });
});

describe(`validateComplexity`, () => {
    let dynamicControl: DynamicControl<any>;
    let validateComplexityValidator: DynamicControlValidator;

    beforeEach(() => {
        dynamicControl = new DynamicControl({});
        validateComplexityValidator = DynamicControlValidators.validateComplexity({
            name: `validateComplexity`,
            errorMessage: testErrorMessage,
            options: null
        });
    });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control value is null, undefined or an empty string`, () => {
            dynamicControl.value = null;
            result = validateComplexityValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = undefined;
            result = validateComplexityValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = ``;
            result = validateComplexityValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control value is sufficiently complex`, () => {
            dynamicControl.value = `aaabbb00`;
            result = validateComplexityValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = `aaAA11@@`;
            result = validateComplexityValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = `1234567890123`;
            result = validateComplexityValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message if 
        control value is not sufficiently complex`, () => {
            dynamicControl.value = `aaaaaaaa`;
            result = validateComplexityValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);

            dynamicControl.value = `@!#$%^&*`;
            result = validateComplexityValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);

            dynamicControl.value = `123456789012`;
            result = validateComplexityValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);
        });
});

describe(`validateDiffers`, () => {
    let dynamicControl: DynamicControl<any>;
    let validateDiffersValidator: DynamicControlValidator;
    let otherDynamicControl: DynamicControl<any>;
    let dynamicForm: DynamicForm;

    beforeEach(() => {
        validateDiffersValidator = DynamicControlValidators.validateDiffers({
            name: `validateDiffers`,
            errorMessage: testErrorMessage,
            options: {
                OtherProperty: ``
            }
        });
        testErrorMessage = `Error message`;
        otherDynamicControl = new DynamicControl({});
        dynamicControl = new DynamicControl({});
        dynamicForm = new DynamicForm([], ``);

        spyOn(dynamicForm, `get`).and.returnValue(otherDynamicControl);
        dynamicControl.parent = dynamicForm;
    });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined if 
        either control value or other value is null, undefined or an empty string`, () => {
            dynamicControl.value = null;
            otherDynamicControl.value = null;
            result = validateDiffersValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = undefined;
            otherDynamicControl.value = undefined;
            result = validateDiffersValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = ``;
            otherDynamicControl.value = ``;
            result = validateDiffersValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            expect(dynamicForm.getDynamicControl).toHaveBeenCalledTimes(3);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control values differ`, () => {
            dynamicControl.value = `test1`;
            otherDynamicControl.value = `test2`;
            result = validateDiffersValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            expect(dynamicForm.getDynamicControl).toHaveBeenCalledTimes(1);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message 
        if control values do not differ`, () => {
            dynamicControl.value = `test`;
            otherDynamicControl.value = `test`;
            result = validateDiffersValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);

            expect(dynamicForm.getDynamicControl).toHaveBeenCalledTimes(1);
        });
});

describe(`validateEmailAddress`, () => {
    let dynamicControl: DynamicControl<any>;
    let validateEmailAddressValidator: DynamicControlValidator;

    beforeEach(() => {
        dynamicControl = new DynamicControl({});
        validateEmailAddressValidator = DynamicControlValidators.validateEmailAddress({
            name: `validateEmailAddress`,
            errorMessage: testErrorMessage,
            options: null
        });
    });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control value is null, undefined or an empty string`, () => {
            dynamicControl.value = null;
            result = validateEmailAddressValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = undefined;
            result = validateEmailAddressValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = ``;
            result = validateEmailAddressValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control value is an email address`, () => {
            dynamicControl.value = `test@test.com`;
            result = validateEmailAddressValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = `test@test`;
            result = validateEmailAddressValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message 
        if control value is not an email address`, () => {
            dynamicControl.value = `test@`;
            result = validateEmailAddressValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);

            dynamicControl.value = `@test`;
            result = validateEmailAddressValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);

            dynamicControl.value = `test@test@test`;
            result = validateEmailAddressValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);

            dynamicControl.value = `test`;
            result = validateEmailAddressValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);
        });
});

describe(`validateLength`, () => {
    let dynamicControl: DynamicControl<any>;
    let validateLengthValidator: DynamicControlValidator;

    beforeEach(() => {
        dynamicControl = new DynamicControl({});
        validateLengthValidator = DynamicControlValidators.validateLength({
            name: `validateLength`,
            errorMessage: testErrorMessage,
            options: {
                Length: `8`
            }
        });
    });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control value is null, undefined or an empty string`, () => {
            dynamicControl.value = null;
            result = validateLengthValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = undefined;
            result = validateLengthValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = ``;
            result = validateLengthValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control value has specified length`, () => {
            dynamicControl.value = `testtest`;
            result = validateLengthValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message 
        if control value does not have specified length`, () => {
            dynamicControl.value = `test`;
            result = validateLengthValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);
        });
});

describe(`validateMatches`, () => {
    let dynamicControl: DynamicControl<any>;
    let validateMatchesValidator: DynamicControlValidator;
    let otherDynamicControl: DynamicControl<any>;
    let dynamicForm: DynamicForm;

    beforeEach(() => {
        otherDynamicControl = new DynamicControl({});
        dynamicControl = new DynamicControl({});
        validateMatchesValidator = DynamicControlValidators.validateMatches({
            name: `validateMatches`,
            errorMessage: testErrorMessage,
            options: {
                OtherProperty: ``
            }
        },
            dynamicControl);
        dynamicForm = new DynamicForm([], ``);

        spyOn(dynamicForm, `get`).and.returnValue(otherDynamicControl);
        dynamicControl.parent = dynamicForm;
    });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if either control value or other value is null, undefined or an empty string`, () => {
            dynamicControl.value = null;
            otherDynamicControl.value = null;
            result = validateMatchesValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = undefined;
            otherDynamicControl.value = undefined;
            result = validateMatchesValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = ``;
            otherDynamicControl.value = ``;
            result = validateMatchesValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            expect(dynamicForm.getDynamicControl).toHaveBeenCalledTimes(3);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control values match`, () => {
            dynamicControl.value = `test`;
            otherDynamicControl.value = `test`;
            result = validateMatchesValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            expect(dynamicForm.getDynamicControl).toHaveBeenCalledTimes(1);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message 
        if control values do not match`, () => {
            dynamicControl.value = `test1`;
            otherDynamicControl.value = `test2`;
            result = validateMatchesValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);

            expect(dynamicForm.getDynamicControl).toHaveBeenCalledTimes(1);
        });
});

describe(`validateMinLength`, () => {
    let dynamicControl: DynamicControl<any>;
    let validateMinLengthValidator: DynamicControlValidator;

    beforeEach(() => {
        dynamicControl = new DynamicControl({});
        validateMinLengthValidator = DynamicControlValidators.validateMinLength({
            name: `validateMinLength`,
            errorMessage: testErrorMessage,
            options: {
                MinLength: `5`
            }
        });
    });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control value is null, undefined or an empty string`, () => {
            dynamicControl.value = null;
            result = validateMinLengthValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = undefined;
            result = validateMinLengthValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = ``;
            result = validateMinLengthValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control value\`s length is greater than or equal to specified minimum length`, () => {
            dynamicControl.value = `testtest`;
            result = validateMinLengthValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = `testtesttest`;
            result = validateMinLengthValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message 
        if control value\`s length is less than specified minimum length length`, () => {
            dynamicControl.value = `test`;
            result = validateMinLengthValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(testErrorMessage);
        });
});

describe(`validateRequired`, () => {
    let dynamicControl: DynamicControl<any>;
    let validateRequiredValidator: DynamicControlValidator;

    beforeEach(() => {
        dynamicControl = new DynamicControl({ displayName: testDisplayName });
        validateRequiredValidator = DynamicControlValidators.validateRequired({
            name: `validateRequired`,
            errorMessage: `{0} testErrorMessage`,
            options: null
        });
    });

    it(`Returns DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message 
        if control value is null, undefined or an empty string`, () => {
            dynamicControl.value = null;
            result = validateRequiredValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(`testDisplayName testErrorMessage`);

            dynamicControl.value = undefined;
            result = validateRequiredValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(`testDisplayName testErrorMessage`);

            dynamicControl.value = ``;
            result = validateRequiredValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(`testDisplayName testErrorMessage`);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined 
        if control value is not null, undefined or an empty string`, () => {
            dynamicControl.value = `test`;
            result = validateRequiredValidator(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);
        });
});

describe(`validateAsync`, () => {
    let dynamicControl: DynamicControl<any>;
    let validateAsyncValidator: DynamicControlAsyncValidator;
    let testDynamicFormsService = new DynamicFormsService(null);

    beforeEach(() => {
        dynamicControl = new DynamicControl({ displayName: testDisplayName });
        validateAsyncValidator = DynamicControlValidators.validateAsync({
            name: `validateAsync`,
            errorMessage: `{0} testErrorMessage`,
            options: null
        },
            dynamicControl,
            null);
        spyOn(testDynamicFormsService, `validateValue`).and.returnValue(Observable.empty<Validity>());
    });

    it(`Returns DynamicControlValidatorResult with validity = Validity.invalid and message = undefined if 
        dynamicControl is invalid before function is called`, () => {
            dynamicControl.validity = Validity.invalid;

            result = validateAsyncValidator.validate(dynamicControl);

            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.invalid);
            expect(result.message).toBe(undefined);
        });

    it(`DynamicControlValidatorResult with validity = Validity.valid and message = undefined if
        control value is null, undefined or an empty string`, () => {
            dynamicControl.validity = Validity.valid;

            dynamicControl.value = null;
            result = validateAsyncValidator.validate(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = undefined;
            result = validateAsyncValidator.validate(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);

            dynamicControl.value = ``;
            result = validateAsyncValidator.validate(dynamicControl);
            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.valid);
            expect(result.message).toBe(undefined);
        });

    it(`Returns DynamicControlValidatorResult with validity = Validity.pending and message = undefined if 
        dynamicControl is valid before function is called`, () => {
            dynamicControl.validity = Validity.valid;
            dynamicControl.value = `test`;

            result = validateAsyncValidator.validate(dynamicControl);

            expect(result).toBeDefined();
            expect(result.validity).toBe(Validity.pending);
            expect(result.message).toBe(undefined);
        });

    // TODO Figure out how to test
    // describe(`Creates subject stream`, () => {
    // });
});
