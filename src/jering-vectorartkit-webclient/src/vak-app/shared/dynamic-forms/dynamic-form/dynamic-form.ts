import { DynamicControl} from '../dynamic-control/dynamic-control';

/**
 * Represents a dynamically generated form.
 */
export class DynamicForm {
    constructor(public dynamicControls: DynamicControl<any>[]) {
        for (let dynamicControl of dynamicControls) {
            dynamicControl.parent = this;
        }
    }

    /**
     * Current form value
     */
    get value(): { [key: string]: string } {
        let result: { [key: string]: string } = {};

        for (let dynamicControl of this.dynamicControls) {
            result[dynamicControl.name] = dynamicControl.value;
        }

        return result;
    }

    /**
     * Returns
     * - DynamicControl with specified name
     */
    get(name: string): DynamicControl<any> {
        return this.dynamicControls.find(dynamicControl => dynamicControl.name === name);
    }

    /**
     * Validates form. Validation errors are added directly to each DynamicControl.
     */
    validate(): void {
        for (let dynamicControl of this.dynamicControls) {
            dynamicControl.validate();
        }
    }
}
