import { DynamicControlValidatorData } from './dynamic-control-validator-data';
import { DynamicControlValidator } from './dynamic-control-validator';
import { DynamicControlValidators } from './dynamic-control-validators';
import { DynamicForm } from '../dynamic-form/dynamic-form';

/**
 * Dynamically generated control
 */
export class DynamicControl<T>{
    name: string;
    displayName: string;
    order: number;
    tagName: string;
    validators: DynamicControlValidator[];
    properties: { [key: string]: string };

    parent: DynamicForm;
    errors: string[];
    value: string;
    dirty: boolean;
    blurred: boolean;
    valid: boolean;

    constructor(options: {
        name?: string,
        displayName?: string,
        order?: number,
        tagName?: string,
        validatorDatas?: DynamicControlValidatorData[],
        properties?: { [key: string]: string }
    } = {}) {
        this.name = options.name || '';
        this.displayName = options.displayName || '';
        this.order = options.order === undefined ? 1 : options.order;
        this.tagName = options.tagName || '';
        this.validators = [];
        let validatorDatas = options.validatorDatas || [];
        for (let validatorData of validatorDatas) {
            this.validators.push(DynamicControlValidators[validatorData.name](validatorData, this));
        }
        this.properties = options.properties || {};
    }

    /**
     * Validates control if it is both dirty and blurred
     */
    validate(): void {
        this.errors = [];
        this.valid = true;
        for (let validator of this.validators) {
            let error = validator(this);
            if (error != null) {
                this.errors.push(error);
                if (this.valid) {
                    this.valid = false;
                }
            }
        }
    }

    /**
     * Updates value, sets dirty to true and triggers validation for all DynamicControls in containing DynamicForm.
     */
    onInput(event: any): void {
        this.value = event.target.value;
        this.dirty = true;
        this.parent.validate();
    }

    /**
     * Sets blurred to true and triggers validation for all DynamicControls in containing DynamicForm.
     */
    onBlur(event: any): void {
        this.blurred = true;
        this.parent.validate();
    }

    /**
     * Updates value, sets dirty and blurred to true and triggers validation for all DynamicControls in contaning DynamicForm.
     */
    onChange(event: any): void {
        this.value = event.target.value;
        this.dirty = true;
        this.blurred = true;
        this.parent.validate();
    }
}
