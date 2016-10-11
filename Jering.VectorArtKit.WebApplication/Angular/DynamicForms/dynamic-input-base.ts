import { DynamicInputValidatorData } from './dynamic-input-validator-data';

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
}
