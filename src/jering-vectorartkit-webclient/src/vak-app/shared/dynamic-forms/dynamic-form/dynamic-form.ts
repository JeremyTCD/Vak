import { DynamicControl } from '../dynamic-control/dynamic-control';
import { Validity } from '../validity';

/**
 * Represents a dynamically generated form.
 */
export class DynamicForm {
    validity: Validity;
    submitAttempted: boolean;
    errors: string[];

    constructor(public dynamicControls: DynamicControl<any>[], public errorMessage: string) {
        for (let dynamicControl of dynamicControls) {
            dynamicControl.setupContext(this);
        }
    }

    /**
     * Validate form and determine whether form is valid.
     *
     * Returns
     * - True if form is valid, false otherwise.
     */
    onSubmit(): boolean {
        this.submitAttempted = true;

        for (let dynamicControl of this.dynamicControls) {
            dynamicControl.dirty = true;
            dynamicControl.blurred = true;
        }

        this.validate();

        this.errors = [];
        if (this.validity === Validity.invalid) {
            this.errors.push(this.errorMessage);
            return false;
        }

        return true;
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
        this.validity = Validity.valid;
        for (let dynamicControl of this.dynamicControls) {
            dynamicControl.validate();
            if (dynamicControl.validity === Validity.invalid) {
                this.validity = Validity.invalid;
            }
        }
    }

    dispose(): void {
        for (let dynamicControl of this.dynamicControls) {
            dynamicControl.dispose();
        }
    }
}
