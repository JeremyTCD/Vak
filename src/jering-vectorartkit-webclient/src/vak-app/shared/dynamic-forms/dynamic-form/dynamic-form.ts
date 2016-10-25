import { DynamicControl} from '../dynamic-control/dynamic-control';

/**
 * Represents a dynamically generated form.
 */
export class DynamicForm {
    valid: boolean;
    submitAttempted: boolean;
    errors: string[];

    constructor(public dynamicControls: DynamicControl<any>[], public errorMessage: string) {
        for (let dynamicControl of dynamicControls) {
            dynamicControl.parent = this;
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
        if (!this.valid) {
            this.errors.push(this.errorMessage);
        }

        return this.valid;
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
        this.valid = true;
        for (let dynamicControl of this.dynamicControls) {
            dynamicControl.validate();
            if (this.valid && !dynamicControl.valid) {
                this.valid = false;
            }
        }
    }
}
