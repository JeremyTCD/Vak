import { DynamicControl } from '../dynamic-control/dynamic-control';
import { Validity } from '../validity';

/**
 * Represents a dynamically generated form.
 */
export class DynamicForm {
    validity: Validity;
    messages: string[];
    submitAttempted: boolean;

    constructor(public dynamicControls: DynamicControl<any>[], public message: string) {
        for (let dynamicControl of dynamicControls) {
            dynamicControl.setupContext(this);
        }
    }

    /**
     * Validates DynamicForm and determines whether it can be submitted.
     *
     * Returns
     * - True if form can be submitted, false otherwise.
     */
    onSubmit(): boolean {
        this.submitAttempted = true;

        for (let dynamicControl of this.dynamicControls) {
            dynamicControl.dirty = true;
            dynamicControl.blurred = true;
        }

        this.validate();

        return this.validity === Validity.valid;
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
     * Validates DynamicForm and its DynamicControls. Sets form validity to invalid if any of its DynamicControls are invalid or pending. Otherwise, sets form validity to valid.
     */
    validate(): void {
        this.validity = Validity.valid;
        this.messages = [];
        for (let dynamicControl of this.dynamicControls) {
            // Force validation of controls that have never been validated
            if (dynamicControl.validity === undefined) {
                dynamicControl.validate();
            }

            // If any control invalid, mark form as invalid
            if (this.validity !== Validity.invalid &&
                dynamicControl.validity === Validity.invalid ||
                dynamicControl.validity === Validity.pending) {
                this.validity = Validity.invalid;
                this.messages.push(this.message);
            }
        }
    }

    dispose(): void {
        for (let dynamicControl of this.dynamicControls) {
            dynamicControl.dispose();
        }
    }
}
