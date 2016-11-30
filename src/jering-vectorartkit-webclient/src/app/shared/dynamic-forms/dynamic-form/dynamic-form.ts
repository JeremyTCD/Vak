import { DynamicControl } from '../dynamic-control/dynamic-control';
import { Validity } from '../validity';

/**
 * Represents a dynamically generated form.
 */
export class DynamicForm {
    validity: Validity = Validity.unknown;
    messages: string[] = [];
    submitAttempted: boolean;

    constructor(public dynamicControls: DynamicControl[], public message: string, public buttonText: string) {
    }

    /**
     * Sets submitAttempted to true, sets dirty and blurred to true for all child DynamicControls,
     * calls validate and determines whether it can be submitted.
     *
     * Returns
     * - True if DynamicForm is valid and can thus be submitted, false otherwise.
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
     *
     * Returns
     * - Child DynamicControl name-value map
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
    getDynamicControl(name: string): DynamicControl {
        return this.dynamicControls.find(dynamicControl => dynamicControl.name === name);
    }

    /**
     * Validates DynamicForm and its DynamicControls. Sets form validity to invalid if any of its DynamicControls are invalid or pending. Otherwise, sets form validity to valid.
     */
    validate(): void {
        let validity = Validity.valid;
        this.messages = [];
        for (let dynamicControl of this.dynamicControls) {
            // Force validation of controls that have never been validated
            if (dynamicControl.validity === undefined) {
                dynamicControl.validate();
            }

            // If any control is invalid, mark form as invalid
            if (validity !== Validity.invalid &&
                dynamicControl.validity === Validity.invalid ||
                dynamicControl.validity === Validity.pending) {
                validity = Validity.invalid;
                this.messages.push(this.message);
                // Cannot break since some DynamicControls may need to be validated
            }
        }
        this.validity = validity;
    }

    /**
     * Sets up context for each child DynamicControl.
     */
    setupContext(): void {
        for (let dynamicControl of this.dynamicControls) {
            dynamicControl.setupContext(this);
        }
    }

    dispose(): void {
        for (let dynamicControl of this.dynamicControls) {
            dynamicControl.dispose();
        }
    }
}
