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
        if (!this.submitAttempted) {
            this.submitAttempted = true;

            for (let dynamicControl of this.dynamicControls) {
                dynamicControl.dirty = true;
                dynamicControl.blurred = true;
            }
        }

        this.validate();

        if (this.validity === Validity.invalid) {
            return false;
        }

        // Unsubscribe from async validators and set controls with validity pending to valid
        if (this.validity === Validity.pending) {
            for (let dynamicControl of this.dynamicControls) {
                if (dynamicControl.isValidityPending()) {
                    dynamicControl.unsubscribeAsyncValidator();
                    dynamicControl.validity = Validity.valid;
                }
            }
        }

        return true;
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
     * Validates DynamicForm and its DynamicControls. Sets validity to invalid if any
     * DynamicControls are invalid. Sets validity to pending if no DynamicControls are
     * invalid and at least one is pending. Otherwise, sets validity to valid.
     */
    validate(): void {
        for (let dynamicControl of this.dynamicControls) {
            // Force validation of controls that have never been validated
            if (dynamicControl.validity === Validity.unknown) {
                dynamicControl.validate();
            }
        }

        this.messages = [];

        if (this.dynamicControls.some(dc => dc.validity === Validity.invalid)) {
            // If any control is invalid, set validity to invalid
            this.validity = Validity.invalid;
            this.messages.push(this.message);
        } else if (this.dynamicControls.some(dc => dc.validity === Validity.pending)) {
            // If no control is invalid and at least one is pending, set validity to pending
            this.validity = Validity.pending;
        } else {
            this.validity = Validity.valid;
        }
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
