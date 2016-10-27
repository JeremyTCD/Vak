import { DynamicControlValidatorData } from './dynamic-control-validator-data';
import { DynamicControlValidator } from './dynamic-control-validator';
import { DynamicControlValidators } from './dynamic-control-validators';
import { DynamicForm } from '../dynamic-form/dynamic-form';
import { DynamicFormsService } from '../dynamic-forms.service';
import { Validity } from '../validity';

/**
 * Dynamically generated control
 */
export class DynamicControl<T>{
    name: string;
    displayName: string;
    order: number;
    tagName: string;
    validators: DynamicControlValidator[];
    asyncValidator: DynamicControlValidator;
    properties: { [key: string]: string };

    parent: DynamicForm;
    messages: string[];
    value: string;
    dirty: boolean;
    blurred: boolean;
    validity: Validity;

    constructor(options: {
        name?: string,
        displayName?: string,
        order?: number,
        tagName?: string,
        validatorData?: DynamicControlValidatorData[],
        asyncValidatorData?: DynamicControlValidatorData,
        properties?: { [key: string]: string }
    } = {}, dynamicFormsService?: DynamicFormsService) {
        this.name = options.name || '';
        this.displayName = options.displayName || '';
        this.order = options.order === undefined ? 1 : options.order;
        this.tagName = options.tagName || '';
        this.validators = [];
        let validatorData = options.validatorData || [];
        for (let validatorDatum of validatorData) {
            this.validators.push(DynamicControlValidators[validatorDatum.name](validatorDatum));
        }
        this.asyncValidator = options.asyncValidatorData ? DynamicControlValidators[options.asyncValidatorData.name](options.asyncValidatorData, this, dynamicFormsService) : null;
        this.properties = options.properties || {};
    }

    /**
     * Validates control if it is both dirty and blurred
     */
    validate(): void {
        this.messages = [];

        this.validity = Validity.valid;
        for (let validator of this.validators) {
            let validatorResult = validator(this);

            if (validatorResult.validity === Validity.invalid) {
                this.validity = Validity.invalid;
            }
            if (validatorResult.message) {
                this.messages.push(validatorResult.message);
            }
        }

        if (this.asyncValidator) {
            let asyncValidatorResult = this.asyncValidator(this);

            this.validity = asyncValidatorResult.validity;
            if (asyncValidatorResult.message) {
                this.messages.push(asyncValidatorResult.message);
            }
        }
    }

    /**
     * Updates value and sets dirty to true. If control has been blurred before, run validation.
     */
    onInput(event: any): void {
        this.value = event.target.value;
        this.dirty = true;
        if (this.blurred) {
            this.validate();
        }
    }

    /**
     * On first blur event, set blurred to true and run validation.
     */
    onBlur(event: any): void {
        if (!this.blurred) {
            this.validate();
            this.blurred = true;
        }
    }

    /**
     * Updates value, sets dirty and blurred to true and triggers validation for all DynamicControls in contaning DynamicForm.
     */
    onChange(event: any): void {
        this.value = event.target.value;
        this.dirty = true;
        this.blurred = true;
        this.validate();
    }

    isValidityPending(): boolean {
        return this.validity === Validity.pending;
    }
}
