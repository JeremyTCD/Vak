import { DynamicInputValidatorData } from './dynamic-input-validator-data';
import { DynamicInputValidator } from './dynamic-input-validators';
import { DynamicForm } from '../dynamic-form/dynamic-form';

/**
 * @Description
 * 
 */
export class DynamicInput<T>{
    initialValue: T;
    name: string;
    displayName: string;
    order: number;
    type: string;
    tagName: string;
    placeholder: string;
    renderLabel: boolean;
    validatorData: DynamicInputValidatorData[];

    errors: string[];
    parent: DynamicForm;
    validators: DynamicInputValidator[];
    value: string;
    dirty: boolean;
    blurred: boolean;

    constructor(options: {
        initialValue?: T,
        name?: string,
        displayName?: string,
        order?: number,
        type?: string,
        tagName?: string,
        placeholder?: string,
        renderLabel?: boolean,
        validatorData?: DynamicInputValidatorData[]
    } = {}) {
        this.initialValue = options.initialValue;
        this.name = options.name || '';
        this.displayName = options.displayName || '';
        this.order = options.order === undefined ? 1 : options.order;
        this.type = options.type || '';
        this.tagName = options.tagName || '';
        this.placeholder = options.placeholder || '';
        this.renderLabel = options.renderLabel || false;
        this.validatorData = options.validatorData || [];
    }

    validate(): void {
        if (this.dirty && this.blurred) {
            this.errors = [];
            for (let validator of this.validators) {
                let error = validator(this);
                if (error != null) {
                    this.errors.push(error);
                }
            }
        }
    }
}
