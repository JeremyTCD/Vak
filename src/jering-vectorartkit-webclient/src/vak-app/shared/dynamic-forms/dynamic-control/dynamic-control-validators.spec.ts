import { DynamicControlValidators } from './dynamic-control-validators';
import { DynamicControlValidator } from './dynamic-control-validator';
import { DynamicControlValidatorData } from './dynamic-control-validator-data';
import { DynamicControlValidatorResult } from './dynamic-control-validator-result';
import { DynamicControl } from './dynamic-control';
import { DynamicForm } from '../dynamic-form/dynamic-form';
import { Validity } from '../validity';

let testErrorMessage = 'testErrorMessage';
let result: DynamicControlValidatorResult;

describe('validateAllDigits', () => {
    let dynamicControl: DynamicControl<any>;
    let dynamicControlValidatorData: DynamicControlValidatorData;
    let validateAllDigitsValidator: DynamicControlValidator;

    beforeEach(() => {
        dynamicControl = new DynamicControl();
        dynamicControlValidatorData = new DynamicControlValidatorData(
            {
                name: 'validateAllDigits',
                errorMessage: testErrorMessage,
                options: null
            });
        validateAllDigitsValidator = DynamicControlValidators.validateAllDigits(dynamicControlValidatorData);
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control value is null, undefined or an empty string', () => {
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

        dynamicControl.value = '';
        result = validateAllDigitsValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control value contains only digits', () => {
        dynamicControl.value = '0';
        result = validateAllDigitsValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);

        dynamicControl.value = '12345';
        result = validateAllDigitsValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);
    });

    it('DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message if control value does not contain only digits', () => {
        dynamicControl.value = '-1';
        result = validateAllDigitsValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);

        dynamicControl.value = '1.1';
        result = validateAllDigitsValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);

        dynamicControl.value = 'a12345';
        result = validateAllDigitsValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);

        dynamicControl.value = 'test';
        result = validateAllDigitsValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);
    });
});

describe('validateComplexity', () => {
    let dynamicControl: DynamicControl<any>;
    let dynamicControlValidatorData: DynamicControlValidatorData;
    let validateComplexityValidator: DynamicControlValidator;

    beforeEach(() => {
        dynamicControl = new DynamicControl();
        dynamicControlValidatorData = new DynamicControlValidatorData(
            {
                name: 'validateComplexity',
                errorMessage: testErrorMessage,
                options: null
            });
        validateComplexityValidator = DynamicControlValidators.validateComplexity(dynamicControlValidatorData);;
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control value is null, undefined or an empty string', () => {
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

        dynamicControl.value = '';
        result = validateComplexityValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control value is sufficiently complex', () => {
        dynamicControl.value = 'aaabbb00';
        result = validateComplexityValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);

        dynamicControl.value = 'aaAA11@@';
        result = validateComplexityValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);

        dynamicControl.value = '1234567890123';
        result = validateComplexityValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);
    });

    it('DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message if control value is not sufficiently complex', () => {
        dynamicControl.value = 'aaaaaaaa';
        result = validateComplexityValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);

        dynamicControl.value = '@!#$%^&*';
        result = validateComplexityValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);

        dynamicControl.value = '123456789012';
        result = validateComplexityValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);
    });
});

describe('validateDiffers', () => {
    let dynamicControl: DynamicControl<any>;
    let dynamicControlValidatorData: DynamicControlValidatorData;
    let validateDiffersValidator: DynamicControlValidator;
    let otherDynamicControl: DynamicControl<any>;
    let dynamicForm: DynamicForm;

    beforeEach(() => {
        dynamicControlValidatorData = new DynamicControlValidatorData(
            {
                name: 'validateDiffers',
                errorMessage: testErrorMessage,
                options: {
                    OtherProperty: ''
                }
            });
        validateDiffersValidator = DynamicControlValidators.validateDiffers(dynamicControlValidatorData);
        testErrorMessage = 'Error message';
        otherDynamicControl = new DynamicControl();
        dynamicControl = new DynamicControl();
        dynamicForm = new DynamicForm([], "");

        spyOn(dynamicForm, 'get').and.returnValue(otherDynamicControl);
        dynamicControl.parent = dynamicForm;
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if either control value or other value is null, undefined or an empty string', () => {
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

        dynamicControl.value = '';
        otherDynamicControl.value = '';
        result = validateDiffersValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);

        expect(dynamicForm.get).toHaveBeenCalledTimes(3);
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control values differ', () => {
        dynamicControl.value = 'test1';
        otherDynamicControl.value = 'test2';
        result = validateDiffersValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);

        expect(dynamicForm.get).toHaveBeenCalledTimes(1);
    });

    it('DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message if control values do not differ', () => {
        dynamicControl.value = 'test';
        otherDynamicControl.value = 'test';
        result = validateDiffersValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);

        expect(dynamicForm.get).toHaveBeenCalledTimes(1);
    });
});

describe('validateEmailAddress', () => {
    let dynamicControl: DynamicControl<any>;
    let dynamicControlValidatorData: DynamicControlValidatorData;
    let validateEmailAddressValidator: DynamicControlValidator;

    beforeEach(() => {
        dynamicControl = new DynamicControl();
        dynamicControlValidatorData = new DynamicControlValidatorData(
            {
                name: 'validateEmailAddress',
                errorMessage: testErrorMessage,
                options: null
            });
        validateEmailAddressValidator = DynamicControlValidators.validateEmailAddress(dynamicControlValidatorData);;
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control value is null, undefined or an empty string', () => {
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

        dynamicControl.value = '';
        result = validateEmailAddressValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control value is an email address', () => {
        dynamicControl.value = 'test@test.com';
        result = validateEmailAddressValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);

        dynamicControl.value = 'test@test';
        result = validateEmailAddressValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);
    });

    it('DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message if control value is not an email address', () => {
        dynamicControl.value = 'test@';
        result = validateEmailAddressValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);

        dynamicControl.value = '@test';
        result = validateEmailAddressValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);

        dynamicControl.value = 'test@test@test';
        result = validateEmailAddressValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);

        dynamicControl.value = 'test';
        result = validateEmailAddressValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);
    });
});

describe('validateLength', () => {
    let dynamicControl: DynamicControl<any>;
    let dynamicControlValidatorData: DynamicControlValidatorData;
    let validateLengthValidator: DynamicControlValidator;

    beforeEach(() => {
        dynamicControl = new DynamicControl();
        dynamicControlValidatorData = new DynamicControlValidatorData(
            {
                name: 'validateLength',
                errorMessage: testErrorMessage,
                options: {
                    Length: '8'
                }
            });
        validateLengthValidator = DynamicControlValidators.validateLength(dynamicControlValidatorData);
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control value is null, undefined or an empty string', () => {
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

        dynamicControl.value = '';
        result = validateLengthValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control value has specified length', () => {
        dynamicControl.value = 'testtest';
        result = validateLengthValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);
    });

    it('DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message if control value does not have specified length', () => {
        dynamicControl.value = 'test';
        result = validateLengthValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);
    });
});

describe('validateMatches', () => {
    let dynamicControl: DynamicControl<any>;
    let dynamicControlValidatorData: DynamicControlValidatorData;
    let validateMatchesValidator: DynamicControlValidator;
    let otherDynamicControl: DynamicControl<any>;
    let dynamicForm: DynamicForm;

    beforeEach(() => {
        dynamicControlValidatorData = new DynamicControlValidatorData(
            {
                name: 'validateMatches',
                errorMessage: testErrorMessage,
                options: {
                    OtherProperty: ''
                }
            });
        validateMatchesValidator = DynamicControlValidators.validateMatches(dynamicControlValidatorData);
        otherDynamicControl = new DynamicControl();
        dynamicControl = new DynamicControl();
        dynamicForm = new DynamicForm([], "");

        spyOn(dynamicForm, 'get').and.returnValue(otherDynamicControl);
        dynamicControl.parent = dynamicForm;
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if either control value or other value is null, undefined or an empty string', () => {
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

        dynamicControl.value = '';
        otherDynamicControl.value = '';
        result = validateMatchesValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);

        expect(dynamicForm.get).toHaveBeenCalledTimes(3);
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control values match', () => {
        dynamicControl.value = 'test';
        otherDynamicControl.value = 'test';
        result = validateMatchesValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);

        expect(dynamicForm.get).toHaveBeenCalledTimes(1);
    });

    it('DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message if control values do not match', () => {
        dynamicControl.value = 'test1';
        otherDynamicControl.value = 'test2';
        result = validateMatchesValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);

        expect(dynamicForm.get).toHaveBeenCalledTimes(1);
    });
});

describe('validateMinLength', () => {
    let dynamicControl: DynamicControl<any>;
    let dynamicControlValidatorData: DynamicControlValidatorData;
    let validateMinLengthValidator: DynamicControlValidator;

    beforeEach(() => {
        dynamicControl = new DynamicControl();
        dynamicControlValidatorData = new DynamicControlValidatorData(
            {
                name: 'validateMinLength',
                errorMessage: testErrorMessage,
                options: {
                    MinLength: '5'
                }
            });
        validateMinLengthValidator = DynamicControlValidators.validateMinLength(dynamicControlValidatorData);
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control value is null, undefined or an empty string', () => {
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

        dynamicControl.value = '';
        result = validateMinLengthValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control value\'s length is greater than or equal to specified minimum length', () => {
        dynamicControl.value = 'testtest';
        result = validateMinLengthValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);

        dynamicControl.value = 'testtesttest';
        result = validateMinLengthValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);
    });

    it('DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message if control value\'s length is less than specified minimum length length', () => {
        dynamicControl.value = 'test';        
        result = validateMinLengthValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(testErrorMessage);
    });
});

describe('validateRequired', () => {
    let dynamicControl: DynamicControl<any>;
    let dynamicControlValidatorData: DynamicControlValidatorData;
    let validateRequiredValidator: DynamicControlValidator;

    beforeEach(() => {
        dynamicControl = new DynamicControl({ displayName: `testDisplayName` });
        dynamicControlValidatorData = new DynamicControlValidatorData(
            {
                name: 'validateRequired',
                errorMessage: `{0} testErrorMessage`,
                options: null
            });
        validateRequiredValidator = DynamicControlValidators.validateRequired(dynamicControlValidatorData);
    });

    it('DynamicControlValidatorResult with validity = Validity.invalid and message set to an error message if control value is null, undefined or an empty string', () => {
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

        dynamicControl.value = '';
        result = validateRequiredValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.invalid);
        expect(result.message).toBe(`testDisplayName testErrorMessage`);
    });

    it('DynamicControlValidatorResult with validity = Validity.valid and message = undefined if control value is not null, undefined or an empty string', () => {
        dynamicControl.value = 'test';
        result = validateRequiredValidator(dynamicControl);
        expect(result).toBeDefined();
        expect(result.validity).toBe(Validity.valid);
        expect(result.message).toBe(undefined);
    });
});

