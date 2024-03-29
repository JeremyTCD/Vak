﻿import { ValidatorData } from 'api/response-models/get-dynamic-form.response-model';
import { DynamicControlData } from 'api/response-models/get-dynamic-form.response-model';
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

    constructor(dynamicControlData: DynamicControlData, dynamicFormsService?: DynamicFormsService) {
        this.name = dynamicControlData.name || '';
        this.displayName = dynamicControlData.displayName || '';
        this.order = dynamicControlData.order === undefined ? 1 : dynamicControlData.order;
        this.tagName = dynamicControlData.tagName || '';
        this.validators = [];
        let validatorData = dynamicControlData.validatorData || [];
        for (let validatorDatum of validatorData) {
            this.validators.push(DynamicControlValidators[validatorDatum.name](validatorDatum, this));
        }
        this.asyncValidator = dynamicControlData.asyncValidatorData ?
            new DynamicControlAsyncValidator(dynamicControlData.asyncValidatorData, this, dynamicFormsService) :
            null;
        this.properties = dynamicControlData.properties || {};
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
        this.value = event.target.checked.toString();
        this.blurred = true;
        this.dirty = true;

        this.validate();
        this.tryValidateParent();
    }

    /**
     * Validates parent if parent is defined and submit has been attempted on it.
     */
    tryValidateParent(): void {
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

    unsubscribeAsyncValidator(): void {
        if (this.asyncValidator) {
            this.asyncValidator.subscription.unsubscribe();
        }
    }

    dispose(): void {
        this.unsubscribeAsyncValidator();
    }
}
