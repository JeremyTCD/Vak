import { ValidatorResponseModel } from '../response-models/validator.response-model';
import { DynamicControlResponseModel } from '../response-models/dynamic-control.response-model';
import { DynamicControlValidator } from './dynamic-control-validator';
import { DynamicControlAsyncValidator } from './dynamic-control-async-validator';
import { DynamicControlValidators } from './dynamic-control-validators';
import { DynamicForm } from '../dynamic-form/dynamic-form';
import { DynamicFormsService } from '../dynamic-forms.service';
import { Validity } from '../validity';

/**
 * Dynamically generated control
 */
export class DynamicControl{
    name: string;
    displayName: string;
    order: number;
    tagName: string;
    validators: DynamicControlValidator[];
    asyncValidator: DynamicControlAsyncValidator;
    properties: { [key: string]: string };

    parent: DynamicForm;
    messages: string[] = [];
    value: string;
    dirty: boolean;
    blurred: boolean;
    providerSiblingsNames: string[] = [];
    dependentSiblings: DynamicControl[] = [];
    validity: Validity = Validity.unknown;

    constructor(controlResponseModel: DynamicControlResponseModel, dynamicFormsService?: DynamicFormsService) {
        this.name = controlResponseModel.name || '';
        this.displayName = controlResponseModel.displayName || '';
        this.order = controlResponseModel.order === undefined ? 1 : controlResponseModel.order;
        this.tagName = controlResponseModel.tagName || '';
        this.validators = [];
        let validatorModels = controlResponseModel.validatorResponseModels || [];
        for (let validatorModel of validatorModels) {
            this.validators.push(DynamicControlValidators[validatorModel.name](validatorModel, this));
        }
        this.asyncValidator = controlResponseModel.asyncValidatorResponseModel ?
            new DynamicControlAsyncValidator(controlResponseModel.asyncValidatorResponseModel, this, dynamicFormsService) :
            null;
        this.properties = controlResponseModel.properties || {};
    }

    /**
     * Validates DynamicControl
     */
    validate(): void {
        this.messages = [];

        let validity = Validity.valid;
        for (let validator of this.validators) {
            let validatorResult = validator(this);

            if (validity !== Validity.invalid &&
                validatorResult.validity === Validity.invalid) {
                validity = Validity.invalid;
            }
            if (validatorResult.message) {
                this.messages.push(validatorResult.message);
            }
        }
        this.validity = validity;

        if (this.asyncValidator) {
            let asyncValidatorResult = this.asyncValidator.validate(this);

            this.validity = asyncValidatorResult.validity;
            if (asyncValidatorResult.message) {
                this.messages.push(asyncValidatorResult.message);
            }
        }

        for (let sibling of this.dependentSiblings) {
            if (sibling.dirty && sibling.blurred) {
                sibling.validate();
            }
        }
    }

    /**
     * Updates value and sets dirty to true. Validates DynamicControl if it has been blurred.
     * Tries to validate parent if DynamicControl validity changes.
     */
    onInput(event: any): void {
        this.value = event.target.value;
        if (this.blurred) {
            this.validate();
            this.tryValidateParent();
        }
        this.dirty = true;
    }

    /**
     * Sets blurred to true and validates DynamicControl on first blur when DynamicControl is dirty.
     * Tries to validate parent if DynamicControl validity changes.
     */
    onBlur(event: any): void {
        if (this.dirty && !this.blurred) {
            this.blurred = true;
            this.validate();
            this.tryValidateParent();
        }
    }

    /**
     * Updates value, sets dirty and blurred to true, calls validate
     * and tryValidateParent.
     */
    onChange(event: any): void {
        this.value = event.target.checked;
        this.blurred = true;
        this.dirty = true;

        this.validate();
        this.tryValidateParent();
    }

    /**
     * Validates parent if parent is defined and submit has been attempted on it.
     */
    tryValidateParent() {
        if (this.parent &&
            this.parent.submitAttempted) {
            this.parent.validate();
        }
    }

    /**
     * Sets hook referencing parent DynamicForm and registers DynamicControl with provider siblings.
     *
     * Errors
     * - RangeError if no sibling corresponds to a provider sibling name.
     */
    setupContext(dynamicForm: DynamicForm): void {
        this.parent = dynamicForm;
        for (let name of this.providerSiblingsNames) {
            let sibling = dynamicForm.getDynamicControl(name);
            if (sibling) {
                sibling.dependentSiblings.push(this);
            } else {
                throw new RangeError(`No sibling with name ${name} exists`);
            }
        }
    }

    /**
     * Returns
     * - True if validity is Validity.pending
     * - False otherwise
     */
    isValidityPending(): boolean {
        return this.validity === Validity.pending;
    }

    dispose(): void {
        if (this.asyncValidator) {
            this.asyncValidator.subscription.unsubscribe();
        }
    }
}
