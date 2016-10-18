import { DynamicInputValidators, DynamicInputValidator } from './dynamic-input-validators';
import { DynamicInputValidatorData } from './dynamic-input-validator-data';
import { DynamicInput } from './dynamic-input';
import { DynamicForm } from '../dynamic-form/dynamic-form';

describe('validateAllDigits', () => {

    let dynamicInputValidatorData = new DynamicInputValidatorData(
        {
            name: 'validateAllDigits',
            errorMessage: 'Error message',
            options: null
        });
    let validateAllDigitsValidator = DynamicInputValidators.validateAllDigits(dynamicInputValidatorData);
    let errorMessage = 'Error message';
    let dynamicInput = new DynamicInput();

    it('returns null if control value is null, undefined or an empty string', () => {
        dynamicInput.value = null;
        expect(validateAllDigitsValidator(dynamicInput)).toBe(null);

        dynamicInput.value = undefined;
        expect(validateAllDigitsValidator(dynamicInput)).toBe(null);

        dynamicInput.value = '';
        expect(validateAllDigitsValidator(dynamicInput)).toBe(null);
    });

    it('returns null if control value contains only digits', () => {
        dynamicInput.value = '0';
        expect(validateAllDigitsValidator(dynamicInput)).toBe(null);

        dynamicInput.value = '12345';
        expect(validateAllDigitsValidator(dynamicInput)).toBe(null);
    });

    it('returns error message if control value does not contain only digits', () => {
        dynamicInput.value = '-1';
        expect(validateAllDigitsValidator(dynamicInput)).toBe(errorMessage);

        dynamicInput.value = '1.1';
        expect(validateAllDigitsValidator(dynamicInput)).toBe(errorMessage);

        dynamicInput.value = 'a12345';
        expect(validateAllDigitsValidator(dynamicInput)).toBe(errorMessage);

        dynamicInput.value = 'test';
        expect(validateAllDigitsValidator(dynamicInput)).toBe(errorMessage);
    });
});

describe('validateComplexity', () => {

    let dynamicInputValidatorData = new DynamicInputValidatorData(
        {
            name: 'validateComplexity',
            errorMessage: 'Error message',
            options: null
        });
    let validateComplexityValidator = DynamicInputValidators.validateComplexity(dynamicInputValidatorData);
    let errorMessage = 'Error message';
    let dynamicInput = new DynamicInput();

    it('returns null if control value is null, undefined or an empty string', () => {
        dynamicInput.value = null;
        expect(validateComplexityValidator(dynamicInput)).toBe(null);

        dynamicInput.value = undefined;
        expect(validateComplexityValidator(dynamicInput)).toBe(null);

        dynamicInput.value = '';
        expect(validateComplexityValidator(dynamicInput)).toBe(null);
    });

    it('returns null if control value is sufficiently complex', () => {
        dynamicInput.value = 'aaabbb00';
        expect(validateComplexityValidator(dynamicInput)).toBe(null);

        dynamicInput.value = 'aaAA11@@';
        expect(validateComplexityValidator(dynamicInput)).toBe(null);

        dynamicInput.value = '1234567890123';
        expect(validateComplexityValidator(dynamicInput)).toBe(null);
    });

    it('returns error message if control value is not sufficiently complex', () => {
        dynamicInput.value = 'aaaaaaaa';
        expect(validateComplexityValidator(dynamicInput)).toBe(errorMessage);

        dynamicInput.value = '@!#$%^&*';
        expect(validateComplexityValidator(dynamicInput)).toBe(errorMessage);

        dynamicInput.value = '123456789012';
        expect(validateComplexityValidator(dynamicInput)).toBe(errorMessage);
    });
});

describe('validateDiffers', () => {

    let dynamicInputValidatorData: DynamicInputValidatorData;
    let validateDiffersValidator: DynamicInputValidator;
    let errorMessage: string;
    let otherDynamicInput: DynamicInput<any>;
    let dynamicInput: DynamicInput<any>;
    let dynamicForm: DynamicForm;

    beforeEach(() => {
        dynamicInputValidatorData = new DynamicInputValidatorData(
            {
                name: 'validateDiffers',
                errorMessage: 'Error message',
                options: {
                    OtherProperty: ''
                }
            });
        validateDiffersValidator = DynamicInputValidators.validateDiffers(dynamicInputValidatorData);
        errorMessage = 'Error message';
        otherDynamicInput = new DynamicInput();
        dynamicInput = new DynamicInput();
        dynamicForm = new DynamicForm([]);

        spyOn(dynamicForm, 'get').and.returnValue(otherDynamicInput);
        dynamicInput.parent = dynamicForm;
    });

    it('returns null if either control value or other value is null, undefined or an empty string', () => {
        dynamicInput.value = null;
        otherDynamicInput.value = null;
        expect(validateDiffersValidator(dynamicInput)).toBe(null);

        dynamicInput.value = undefined;
        otherDynamicInput.value = undefined;
        expect(validateDiffersValidator(dynamicInput)).toBe(null);

        dynamicInput.value = '';
        otherDynamicInput.value = '';
        expect(validateDiffersValidator(dynamicInput)).toBe(null);

        expect(dynamicForm.get).toHaveBeenCalledTimes(3);
    });

    it('returns null if control values differ', () => {
        dynamicInput.value = 'test1';
        otherDynamicInput.value = 'test2';
        expect(validateDiffersValidator(dynamicInput)).toBe(null);

        expect(dynamicForm.get).toHaveBeenCalledTimes(1);
    });

    it('returns error message if control values do not differ', () => {
        dynamicInput.value = 'test';
        otherDynamicInput.value = 'test';
        expect(validateDiffersValidator(dynamicInput)).toBe(errorMessage);

        expect(dynamicForm.get).toHaveBeenCalledTimes(1);
    });
});

describe('validateEmailAddress', () => {

    let dynamicInputValidatorData = new DynamicInputValidatorData(
        {
            name: 'validateEmailAddress',
            errorMessage: 'Error message',
            options: null
        });
    let validateEmailAddressValidator = DynamicInputValidators.validateEmailAddress(dynamicInputValidatorData);
    let errorMessage = 'Error message';
    let dynamicInput = new DynamicInput();

    it('returns null if control value is null, undefined or an empty string', () => {
        dynamicInput.value = null;
        expect(validateEmailAddressValidator(dynamicInput)).toBe(null);

        dynamicInput.value = undefined;
        expect(validateEmailAddressValidator(dynamicInput)).toBe(null);

        dynamicInput.value = '';
        expect(validateEmailAddressValidator(dynamicInput)).toBe(null);
    });

    it('returns null if control value is an email address', () => {
        dynamicInput.value = 'test@test.com';
        expect(validateEmailAddressValidator(dynamicInput)).toBe(null);

        dynamicInput.value = 'test@test';
        expect(validateEmailAddressValidator(dynamicInput)).toBe(null);
    });

    it('returns error message if control value is not an email address', () => {
        dynamicInput.value = 'test@';
        expect(validateEmailAddressValidator(dynamicInput)).toBe(errorMessage);

        dynamicInput.value = '@test';
        expect(validateEmailAddressValidator(dynamicInput)).toBe(errorMessage);

        dynamicInput.value = 'test@test@test';
        expect(validateEmailAddressValidator(dynamicInput)).toBe(errorMessage);

        dynamicInput.value = 'test';
        expect(validateEmailAddressValidator(dynamicInput)).toBe(errorMessage);
    });
});

describe('validateLength', () => {
    let dynamicInputValidatorData: DynamicInputValidatorData;
    let validateLengthValidator: DynamicInputValidator;
    let errorMessage: string;
    let dynamicInput: DynamicInput<any>;

    beforeEach(() => {
        dynamicInputValidatorData = new DynamicInputValidatorData(
            {
                name: 'validateLength',
                errorMessage: 'Error message',
                options: {
                    Length: '8'
                }
            });
        validateLengthValidator = DynamicInputValidators.validateLength(dynamicInputValidatorData);
        errorMessage = 'Error message';
        dynamicInput = new DynamicInput();
    });

    it('returns null if control value is null, undefined or an empty string', () => {
        dynamicInput.value = null;
        expect(validateLengthValidator(dynamicInput)).toBe(null);

        dynamicInput.value = undefined;
        expect(validateLengthValidator(dynamicInput)).toBe(null);

        dynamicInput.value = '';
        expect(validateLengthValidator(dynamicInput)).toBe(null);
    });

    it('returns null if control value has specified length', () => {
        dynamicInput.value = 'testtest';
        expect(validateLengthValidator(dynamicInput)).toBe(null);
    });

    it('returns error message if control value does not have specified length', () => {
        dynamicInput.value = 'test';
        expect(validateLengthValidator(dynamicInput)).toBe(errorMessage);
    });
});

describe('validateMatches', () => {

    let dynamicInputValidatorData: DynamicInputValidatorData;
    let validateMatchesValidator: DynamicInputValidator;
    let errorMessage: string;
    let otherDynamicInput: DynamicInput<any>;
    let dynamicInput: DynamicInput <any>;
    let dynamicForm: DynamicForm;

    beforeEach(() => {
        dynamicInputValidatorData = new DynamicInputValidatorData(
            {
                name: 'validateMatches',
                errorMessage: 'Error message',
                options: {
                    OtherProperty: ''
                }
            });
        validateMatchesValidator = DynamicInputValidators.validateMatches(dynamicInputValidatorData);
        errorMessage = 'Error message';
        otherDynamicInput = new DynamicInput();
        dynamicInput = new DynamicInput();
        dynamicForm = new DynamicForm([]);

        spyOn(dynamicForm, 'get').and.returnValue(otherDynamicInput);
        dynamicInput.parent = dynamicForm;
    });

    it('returns null if either control value or other value is null, undefined or an empty string', () => {
        dynamicInput.value = null;
        otherDynamicInput.value = null;
        expect(validateMatchesValidator(dynamicInput)).toBe(null);

        dynamicInput.value = undefined;
        otherDynamicInput.value = undefined;
        expect(validateMatchesValidator(dynamicInput)).toBe(null);

        dynamicInput.value = '';
        otherDynamicInput.value = '';
        expect(validateMatchesValidator(dynamicInput)).toBe(null);

        expect(dynamicForm.get).toHaveBeenCalledTimes(3);
    });

    it('returns null if control values match', () => {
        dynamicInput.value = 'test';
        otherDynamicInput.value = 'test';
        expect(validateMatchesValidator(dynamicInput)).toBe(null);

        expect(dynamicForm.get).toHaveBeenCalledTimes(1);
    });

    it('returns error message if control values do not match', () => {
        dynamicInput.value = 'test1';
        otherDynamicInput.value = 'test2';
        expect(validateMatchesValidator(dynamicInput)).toBe(errorMessage);

        expect(dynamicForm.get).toHaveBeenCalledTimes(1);
    });
});

describe('validateMinLength', () => {
    let dynamicInputValidatorData: DynamicInputValidatorData;
    let validateMinLengthValidator: DynamicInputValidator;
    let errorMessage: string;
    let dynamicInput: DynamicInput<any>;

    beforeEach(() => {
        dynamicInputValidatorData = new DynamicInputValidatorData(
            {
                name: 'validateMinLength',
                errorMessage: 'Error message',
                options: {
                    MinLength: '5'
                }
            });
        validateMinLengthValidator = DynamicInputValidators.validateMinLength(dynamicInputValidatorData);
        errorMessage = 'Error message';
        dynamicInput = new DynamicInput();
    });

    it('returns null if control value is null, undefined or an empty string', () => {
        dynamicInput.value = null;
        expect(validateMinLengthValidator(dynamicInput)).toBe(null);

        dynamicInput.value = undefined;
        expect(validateMinLengthValidator(dynamicInput)).toBe(null);

        dynamicInput.value = '';
        expect(validateMinLengthValidator(dynamicInput)).toBe(null);
    });

    it('null if control value\'s length is greater than or equal to specified minimum length', () => {
        dynamicInput.value = 'testtest';
        expect(validateMinLengthValidator(dynamicInput)).toBe(null);

        dynamicInput.value = 'testtesttest';
        expect(validateMinLengthValidator(dynamicInput)).toBe(null);
    });

    it('error message if control value\'s length is less than specified minimum length length', () => {
        dynamicInput.value = 'test';
        expect(validateMinLengthValidator(dynamicInput)).toBe(errorMessage);
    });
});

describe('validateRequired', () => {

    let dynamicInputValidatorData = new DynamicInputValidatorData(
        {
            name: 'validateRequired',
            errorMessage: '{0} is required.',
            options: null
        });
    let dynamicInput = new DynamicInput({ displayName: 'Field' });
    let validateRequiredValidator = DynamicInputValidators.validateRequired(dynamicInputValidatorData);
    let errorMessage = dynamicInputValidatorData.errorMessage.replace('{0}', dynamicInput.displayName);

    it('returns error message if control value is null, undefined or an empty string', () => {
        dynamicInput.value = null;
        expect(validateRequiredValidator(dynamicInput)).toBe(errorMessage);

        dynamicInput.value = undefined;
        expect(validateRequiredValidator(dynamicInput)).toBe(errorMessage);

        dynamicInput.value = '';
        expect(validateRequiredValidator(dynamicInput)).toBe(errorMessage);
    });

    it('returns null if control value is not null, undefined or an empty string', () => {
        dynamicInput.value = 'test';
        expect(validateRequiredValidator(dynamicInput)).toBe(null);
    });
});

