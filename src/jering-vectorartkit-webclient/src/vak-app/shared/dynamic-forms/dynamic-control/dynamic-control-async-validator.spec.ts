import { DynamicControlAsyncValidator } from './dynamic-control-async-validator';
import { DynamicControl } from './dynamic-control';
import { Validity } from '../validity';
import { DynamicControlValidatorResult } from './dynamic-control-validator-result';

let testErrorMessage = `testErrorMessage`;
let testDisplayName = `testDisplayName`;

describe(`DynamicControlAsyncValidator`, () => {
    let validateAsyncValidator: DynamicControlAsyncValidator;

    it(`Constructor creates an open subscription`, () => {
        validateAsyncValidator = new DynamicControlAsyncValidator({
            name: `validateAsync`,
            errorMessage: testErrorMessage,
            options: null
        },
            null,
            null);

        expect(validateAsyncValidator.subscription.closed).toBe(false);
    });

    describe(`validate`, () => {
        let dynamicControl: DynamicControl<any>;

        beforeEach(() => {
            dynamicControl = new DynamicControl({ displayName: testDisplayName });
            validateAsyncValidator = new DynamicControlAsyncValidator({
                name: `validateAsync`,
                errorMessage: `{0} testErrorMessage`,
                options: null
            },
                dynamicControl,
                null);
        });

        it(`Returns DynamicControlValidatorResult with validity = Validity.invalid and message = undefined if 
            dynamicControl is invalid before function is called. Unsubscribes from subscription.`, () => {
                dynamicControl.validity = Validity.invalid;

                let result = validateAsyncValidator.validate(dynamicControl);

                expect(validateAsyncValidator.subscription.closed).toBe(true);
                expect(result).toBeDefined();
                expect(result.validity).toBe(Validity.invalid);
                expect(result.message).toBe(undefined);
            });

        it(`Returns DynamicControlValidatorResult with validity = Validity.valid and message = undefined if
            control value is null, undefined or an empty string. Unsubscribes from subscription.`, () => {
                let result: DynamicControlValidatorResult;

                dynamicControl.validity = Validity.valid;

                dynamicControl.value = null;
                result = validateAsyncValidator.validate(dynamicControl);
                expect(validateAsyncValidator.subscription.closed).toBe(true);
                expect(result).toBeDefined();
                expect(result.validity).toBe(Validity.valid);
                expect(result.message).toBe(undefined);

                dynamicControl.value = undefined;
                result = validateAsyncValidator.validate(dynamicControl);
                expect(validateAsyncValidator.subscription.closed).toBe(true);
                expect(result).toBeDefined();
                expect(result.validity).toBe(Validity.valid);
                expect(result.message).toBe(undefined);

                dynamicControl.value = ``;
                result = validateAsyncValidator.validate(dynamicControl);
                expect(validateAsyncValidator.subscription.closed).toBe(true);
                expect(result).toBeDefined();
                expect(result.validity).toBe(Validity.valid);
                expect(result.message).toBe(undefined);
            });

        it(`Returns DynamicControlValidatorResult with validity = Validity.pending and message = undefined if 
            dynamicControl is valid before function is called. Ensures that subscription is open.`, () => {
                validateAsyncValidator.subscription.unsubscribe();
                dynamicControl.validity = Validity.valid;
                dynamicControl.value = `test`;

                let result = validateAsyncValidator.validate(dynamicControl);

                expect(validateAsyncValidator.subscription.closed).toBe(false);
                expect(result).toBeDefined();
                expect(result.validity).toBe(Validity.pending);
                expect(result.message).toBe(undefined);
            });
    });
});