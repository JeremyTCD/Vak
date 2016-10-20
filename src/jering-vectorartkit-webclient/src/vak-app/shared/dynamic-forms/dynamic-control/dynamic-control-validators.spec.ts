import { DynamicControlValidators } from './dynamic-control-validators';
import { DynamicControlValidator } from './dynamic-control-validator';
import { DynamicControlValidatorData } from './dynamic-control-validator-data';
import { DynamicControl } from './dynamic-control';
import { DynamicForm } from '../dynamic-form/dynamic-form';

describe('validateAllDigits', () => {

    let dynamicControlValidatorData = new DynamicControlValidatorData(
        {
            name: 'validateAllDigits',
            errorMessage: 'Error message',
            options: null
        });
    let validateAllDigitsValidator = DynamicControlValidators.validateAllDigits(dynamicControlValidatorData);
    let errorMessage = 'Error message';
    let dynamicControl = new DynamicControl();

    it('returns null if control value is null, undefined or an empty string', () => {
        dynamicControl.value = null;
        expect(validateAllDigitsValidator(dynamicControl)).toBe(null);

        dynamicControl.value = undefined;
        expect(validateAllDigitsValidator(dynamicControl)).toBe(null);

        dynamicControl.value = '';
        expect(validateAllDigitsValidator(dynamicControl)).toBe(null);
    });

    it('returns null if control value contains only digits', () => {
        dynamicControl.value = '0';
        expect(validateAllDigitsValidator(dynamicControl)).toBe(null);

        dynamicControl.value = '12345';
        expect(validateAllDigitsValidator(dynamicControl)).toBe(null);
    });

    it('returns error message if control value does not contain only digits', () => {
        dynamicControl.value = '-1';
        expect(validateAllDigitsValidator(dynamicControl)).toBe(errorMessage);

        dynamicControl.value = '1.1';
        expect(validateAllDigitsValidator(dynamicControl)).toBe(errorMessage);

        dynamicControl.value = 'a12345';
        expect(validateAllDigitsValidator(dynamicControl)).toBe(errorMessage);

        dynamicControl.value = 'test';
        expect(validateAllDigitsValidator(dynamicControl)).toBe(errorMessage);
    });
});

describe('validateComplexity', () => {

    let dynamicControlValidatorData = new DynamicControlValidatorData(
        {
            name: 'validateComplexity',
            errorMessage: 'Error message',
            options: null
        });
    let validateComplexityValidator = DynamicControlValidators.validateComplexity(dynamicControlValidatorData);
    let errorMessage = 'Error message';
    let dynamicControl = new DynamicControl();

    it('returns null if control value is null, undefined or an empty string', () => {
        dynamicControl.value = null;
        expect(validateComplexityValidator(dynamicControl)).toBe(null);

        dynamicControl.value = undefined;
        expect(validateComplexityValidator(dynamicControl)).toBe(null);

        dynamicControl.value = '';
        expect(validateComplexityValidator(dynamicControl)).toBe(null);
    });

    it('returns null if control value is sufficiently complex', () => {
        dynamicControl.value = 'aaabbb00';
        expect(validateComplexityValidator(dynamicControl)).toBe(null);

        dynamicControl.value = 'aaAA11@@';
        expect(validateComplexityValidator(dynamicControl)).toBe(null);

        dynamicControl.value = '1234567890123';
        expect(validateComplexityValidator(dynamicControl)).toBe(null);
    });

    it('returns error message if control value is not sufficiently complex', () => {
        dynamicControl.value = 'aaaaaaaa';
        expect(validateComplexityValidator(dynamicControl)).toBe(errorMessage);

        dynamicControl.value = '@!#$%^&*';
        expect(validateComplexityValidator(dynamicControl)).toBe(errorMessage);

        dynamicControl.value = '123456789012';
        expect(validateComplexityValidator(dynamicControl)).toBe(errorMessage);
    });
});

describe('validateDiffers', () => {

    let dynamicControlValidatorData: DynamicControlValidatorData;
    let validateDiffersValidator: DynamicControlValidator;
    let errorMessage: string;
    let otherDynamicControl: DynamicControl<any>;
    let dynamicControl: DynamicControl<any>;
    let dynamicForm: DynamicForm;

    beforeEach(() => {
        dynamicControlValidatorData = new DynamicControlValidatorData(
            {
                name: 'validateDiffers',
                errorMessage: 'Error message',
                options: {
                    OtherProperty: ''
                }
            });
        validateDiffersValidator = DynamicControlValidators.validateDiffers(dynamicControlValidatorData);
        errorMessage = 'Error message';
        otherDynamicControl = new DynamicControl();
        dynamicControl = new DynamicControl();
        dynamicForm = new DynamicForm([]);

        spyOn(dynamicForm, 'get').and.returnValue(otherDynamicControl);
        dynamicControl.parent = dynamicForm;
    });

    it('returns null if either control value or other value is null, undefined or an empty string', () => {
        dynamicControl.value = null;
        otherDynamicControl.value = null;
        expect(validateDiffersValidator(dynamicControl)).toBe(null);

        dynamicControl.value = undefined;
        otherDynamicControl.value = undefined;
        expect(validateDiffersValidator(dynamicControl)).toBe(null);

        dynamicControl.value = '';
        otherDynamicControl.value = '';
        expect(validateDiffersValidator(dynamicControl)).toBe(null);

        expect(dynamicForm.get).toHaveBeenCalledTimes(3);
    });

    it('returns null if control values differ', () => {
        dynamicControl.value = 'test1';
        otherDynamicControl.value = 'test2';
        expect(validateDiffersValidator(dynamicControl)).toBe(null);

        expect(dynamicForm.get).toHaveBeenCalledTimes(1);
    });

    it('returns error message if control values do not differ', () => {
        dynamicControl.value = 'test';
        otherDynamicControl.value = 'test';
        expect(validateDiffersValidator(dynamicControl)).toBe(errorMessage);

        expect(dynamicForm.get).toHaveBeenCalledTimes(1);
    });
});

describe('validateEmailAddress', () => {

    let dynamicControlValidatorData = new DynamicControlValidatorData(
        {
            name: 'validateEmailAddress',
            errorMessage: 'Error message',
            options: null
        });
    let validateEmailAddressValidator = DynamicControlValidators.validateEmailAddress(dynamicControlValidatorData);
    let errorMessage = 'Error message';
    let dynamicControl = new DynamicControl();

    it('returns null if control value is null, undefined or an empty string', () => {
        dynamicControl.value = null;
        expect(validateEmailAddressValidator(dynamicControl)).toBe(null);

        dynamicControl.value = undefined;
        expect(validateEmailAddressValidator(dynamicControl)).toBe(null);

        dynamicControl.value = '';
        expect(validateEmailAddressValidator(dynamicControl)).toBe(null);
    });

    it('returns null if control value is an email address', () => {
        dynamicControl.value = 'test@test.com';
        expect(validateEmailAddressValidator(dynamicControl)).toBe(null);

        dynamicControl.value = 'test@test';
        expect(validateEmailAddressValidator(dynamicControl)).toBe(null);
    });

    it('returns error message if control value is not an email address', () => {
        dynamicControl.value = 'test@';
        expect(validateEmailAddressValidator(dynamicControl)).toBe(errorMessage);

        dynamicControl.value = '@test';
        expect(validateEmailAddressValidator(dynamicControl)).toBe(errorMessage);

        dynamicControl.value = 'test@test@test';
        expect(validateEmailAddressValidator(dynamicControl)).toBe(errorMessage);

        dynamicControl.value = 'test';
        expect(validateEmailAddressValidator(dynamicControl)).toBe(errorMessage);
    });
});

describe('validateLength', () => {
    let dynamicControlValidatorData: DynamicControlValidatorData;
    let validateLengthValidator: DynamicControlValidator;
    let errorMessage: string;
    let dynamicControl: DynamicControl<any>;

    beforeEach(() => {
        dynamicControlValidatorData = new DynamicControlValidatorData(
            {
                name: 'validateLength',
                errorMessage: 'Error message',
                options: {
                    Length: '8'
                }
            });
        validateLengthValidator = DynamicControlValidators.validateLength(dynamicControlValidatorData);
        errorMessage = 'Error message';
        dynamicControl = new DynamicControl();
    });

    it('returns null if control value is null, undefined or an empty string', () => {
        dynamicControl.value = null;
        expect(validateLengthValidator(dynamicControl)).toBe(null);

        dynamicControl.value = undefined;
        expect(validateLengthValidator(dynamicControl)).toBe(null);

        dynamicControl.value = '';
        expect(validateLengthValidator(dynamicControl)).toBe(null);
    });

    it('returns null if control value has specified length', () => {
        dynamicControl.value = 'testtest';
        expect(validateLengthValidator(dynamicControl)).toBe(null);
    });

    it('returns error message if control value does not have specified length', () => {
        dynamicControl.value = 'test';
        expect(validateLengthValidator(dynamicControl)).toBe(errorMessage);
    });
});

describe('validateMatches', () => {

    let dynamicControlValidatorData: DynamicControlValidatorData;
    let validateMatchesValidator: DynamicControlValidator;
    let errorMessage: string;
    let otherDynamicControl: DynamicControl<any>;
    let dynamicControl: DynamicControl <any>;
    let dynamicForm: DynamicForm;

    beforeEach(() => {
        dynamicControlValidatorData = new DynamicControlValidatorData(
            {
                name: 'validateMatches',
                errorMessage: 'Error message',
                options: {
                    OtherProperty: ''
                }
            });
        validateMatchesValidator = DynamicControlValidators.validateMatches(dynamicControlValidatorData);
        errorMessage = 'Error message';
        otherDynamicControl = new DynamicControl();
        dynamicControl = new DynamicControl();
        dynamicForm = new DynamicForm([]);

        spyOn(dynamicForm, 'get').and.returnValue(otherDynamicControl);
        dynamicControl.parent = dynamicForm;
    });

    it('returns null if either control value or other value is null, undefined or an empty string', () => {
        dynamicControl.value = null;
        otherDynamicControl.value = null;
        expect(validateMatchesValidator(dynamicControl)).toBe(null);

        dynamicControl.value = undefined;
        otherDynamicControl.value = undefined;
        expect(validateMatchesValidator(dynamicControl)).toBe(null);

        dynamicControl.value = '';
        otherDynamicControl.value = '';
        expect(validateMatchesValidator(dynamicControl)).toBe(null);

        expect(dynamicForm.get).toHaveBeenCalledTimes(3);
    });

    it('returns null if control values match', () => {
        dynamicControl.value = 'test';
        otherDynamicControl.value = 'test';
        expect(validateMatchesValidator(dynamicControl)).toBe(null);

        expect(dynamicForm.get).toHaveBeenCalledTimes(1);
    });

    it('returns error message if control values do not match', () => {
        dynamicControl.value = 'test1';
        otherDynamicControl.value = 'test2';
        expect(validateMatchesValidator(dynamicControl)).toBe(errorMessage);

        expect(dynamicForm.get).toHaveBeenCalledTimes(1);
    });
});

describe('validateMinLength', () => {
    let dynamicControlValidatorData: DynamicControlValidatorData;
    let validateMinLengthValidator: DynamicControlValidator;
    let errorMessage: string;
    let dynamicControl: DynamicControl<any>;

    beforeEach(() => {
        dynamicControlValidatorData = new DynamicControlValidatorData(
            {
                name: 'validateMinLength',
                errorMessage: 'Error message',
                options: {
                    MinLength: '5'
                }
            });
        validateMinLengthValidator = DynamicControlValidators.validateMinLength(dynamicControlValidatorData);
        errorMessage = 'Error message';
        dynamicControl = new DynamicControl();
    });

    it('returns null if control value is null, undefined or an empty string', () => {
        dynamicControl.value = null;
        expect(validateMinLengthValidator(dynamicControl)).toBe(null);

        dynamicControl.value = undefined;
        expect(validateMinLengthValidator(dynamicControl)).toBe(null);

        dynamicControl.value = '';
        expect(validateMinLengthValidator(dynamicControl)).toBe(null);
    });

    it('null if control value\'s length is greater than or equal to specified minimum length', () => {
        dynamicControl.value = 'testtest';
        expect(validateMinLengthValidator(dynamicControl)).toBe(null);

        dynamicControl.value = 'testtesttest';
        expect(validateMinLengthValidator(dynamicControl)).toBe(null);
    });

    it('error message if control value\'s length is less than specified minimum length length', () => {
        dynamicControl.value = 'test';
        expect(validateMinLengthValidator(dynamicControl)).toBe(errorMessage);
    });
});

describe('validateRequired', () => {

    let dynamicControlValidatorData = new DynamicControlValidatorData(
        {
            name: 'validateRequired',
            errorMessage: '{0} is required.',
            options: null
        });
    let dynamicControl = new DynamicControl({ displayName: 'Field' });
    let validateRequiredValidator = DynamicControlValidators.validateRequired(dynamicControlValidatorData);
    let errorMessage = dynamicControlValidatorData.errorMessage.replace('{0}', dynamicControl.displayName);

    it('returns error message if control value is null, undefined or an empty string', () => {
        dynamicControl.value = null;
        expect(validateRequiredValidator(dynamicControl)).toBe(errorMessage);

        dynamicControl.value = undefined;
        expect(validateRequiredValidator(dynamicControl)).toBe(errorMessage);

        dynamicControl.value = '';
        expect(validateRequiredValidator(dynamicControl)).toBe(errorMessage);
    });

    it('returns null if control value is not null, undefined or an empty string', () => {
        dynamicControl.value = 'test';
        expect(validateRequiredValidator(dynamicControl)).toBe(null);
    });
});

