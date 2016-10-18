import { DynamicInput} from '../dynamic-input/dynamic-input';

/**
 * @Description
 * 
 */
export class DynamicForm {
    constructor(public dynamicInputs: DynamicInput<any>[]) {
        for (let dynamicInput of dynamicInputs) {
            dynamicInput.parent = this;
        }
    }

    get(name: string): DynamicInput<any> {
        return this.dynamicInputs.find(dynamicInput => dynamicInput.name === name);
    }

    validate(): void {
        for (let dynamicInput of this.dynamicInputs) {
            dynamicInput.validate();
        }
    }
}
