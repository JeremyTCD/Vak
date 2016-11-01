import { DynamicControl } from './dynamic-control';
import { DynamicControlValidator } from './dynamic-control-validator';
import { DynamicControlAsyncValidator } from './dynamic-control-async-validator';
import { DynamicControlValidatorResult } from './dynamic-control-validator-result';
import { DynamicControlValidatorData } from './dynamic-control-validator-data';
import { DynamicForm } from '../dynamic-form/dynamic-form';
import { Validity } from '../validity';
import { StubDomEvent, StubDomElement } from '../../../../testing/dom-stubs';

let dynamicControl: DynamicControl<any>;
let testDynamicForm: DynamicForm;
let testValue = `testValue`;
let testDynamicControlName = `testDynamicControlName`;
let testValidatorMessage = `testValidatorMessage`;

describe('DynamicControl', () => {
    beforeEach(() => {
        dynamicControl = new DynamicControl({});
    });

    it(`onInput sets value, sets dirty to true and calls validate if blurred is truthy`, () => {
        let stubInputEvent = new StubDomEvent(new StubDomElement(testValue));
        dynamicControl.blurred = true;
        spyOn(dynamicControl, `validate`);

        dynamicControl.onInput(stubInputEvent);

        expect(dynamicControl.value).toBe(testValue);
        expect(dynamicControl.dirty).toBe(true);
        expect(dynamicControl.validate).toHaveBeenCalledTimes(1);
    });

    it(`onInput sets value, sets dirty to true but does not call validate if blurred is falsey`, () => {
        let stubInputEvent = new StubDomEvent(new StubDomElement(testValue));
        spyOn(dynamicControl, `validate`);

        dynamicControl.onInput(stubInputEvent);

        expect(dynamicControl.value).toBe(testValue);
        expect(dynamicControl.dirty).toBe(true);
        expect(dynamicControl.validate).not.toHaveBeenCalled();
    });

    it(`onBlur sets blurred to true and calls validate if dirty is truthy and blurred is falsey`, () => {
        let stubInputEvent = new StubDomEvent(new StubDomElement(testValue));
        dynamicControl.dirty = true;
        spyOn(dynamicControl, `validate`);

        dynamicControl.onBlur(stubInputEvent);

        expect(dynamicControl.blurred).toBe(true);
        expect(dynamicControl.validate).toHaveBeenCalledTimes(1);
    });

    it(`onBlur does nothing if dirty is falsey`, () => {
        let stubInputEvent = new StubDomEvent(new StubDomElement(testValue));
        spyOn(dynamicControl, `validate`);

        dynamicControl.onBlur(stubInputEvent);

        expect(dynamicControl.blurred).not.toBeDefined();
        expect(dynamicControl.validate).not.toHaveBeenCalled();
    });

    it(`onBlur does nothing if blurred is truthy`, () => {
        let stubInputEvent = new StubDomEvent(new StubDomElement(testValue));
        dynamicControl.blurred = true;
        spyOn(dynamicControl, `validate`);

        dynamicControl.onBlur(stubInputEvent);

        expect(dynamicControl.validate).not.toHaveBeenCalled();
    });

    it(`setupContext sets parent and registers with provider siblings`, () => {
        dynamicControl.providerSiblingsNames.push(testDynamicControlName);
        let testDynamicControl = new DynamicControl({ name: testDynamicControlName });
        testDynamicForm = new DynamicForm([testDynamicControl], null);
        spyOn(testDynamicForm, `get`).and.returnValue(testDynamicControl);

        dynamicControl.setupContext(testDynamicForm);

        expect(testDynamicForm.getDynamicControl).toHaveBeenCalledTimes(1);
        expect(dynamicControl.parent).toBe(testDynamicForm);
        expect(testDynamicControl.dependentSiblings[0]).toBe(dynamicControl);
    });

    it(`setupContext sets parent but throws error if sibling does not exist `, () => {
        dynamicControl.providerSiblingsNames.push(testDynamicControlName);
        testDynamicForm = new DynamicForm([], null);
        spyOn(testDynamicForm, `get`).and.returnValue(null);

        expect(() => dynamicControl.setupContext(testDynamicForm)).toThrow(new RangeError(`No sibling with name ${testDynamicControlName} exists`));
        expect(dynamicControl.parent).toBe(testDynamicForm);
        expect(testDynamicForm.getDynamicControl).toHaveBeenCalledTimes(1);
    });

    it(`validate sets validity to invalid and adds message if any synchronous validator returns invalid and a message`, () => {
        dynamicControl.validators.push(stubValidator(Validity.invalid, testValidatorMessage));

        dynamicControl.validate();

        expect(dynamicControl.validity).toBe(Validity.invalid);
        expect(dynamicControl.messages[0]).toBe(testValidatorMessage);
    });

    it(`validate sets validity to valid and clears messages if all synchronous validators return valid and no message and there is not asynchronous validator`, () => {
        dynamicControl.validators.push(stubValidator(Validity.valid, null));

        dynamicControl.validate();

        expect(dynamicControl.validity).toBe(Validity.valid);
        expect(dynamicControl.messages.length).toBe(0);
    });

    it(`validate calls asyncValidator.validate, sets validity to the returned validity and adds returned message if asyncValidator is defined`, () => {
        dynamicControl.asyncValidator = stubAsyncValidator(Validity.pending, testValidatorMessage);
        spyOn(dynamicControl.asyncValidator, `validate`).and.callThrough();

        dynamicControl.validate();

        expect(dynamicControl.asyncValidator.validate).toHaveBeenCalledTimes(1);
        expect(dynamicControl.validity).toBe(Validity.pending);
        expect(dynamicControl.messages[0]).toBe(testValidatorMessage);
    });

    it(`validate calls validate on dependent siblings that are dirty and have been blurred`, () => {
        let testDynamicControl = new DynamicControl({ name: testDynamicControlName });
        testDynamicControl.blurred = true;
        testDynamicControl.dirty = true;
        dynamicControl.dependentSiblings.push(testDynamicControl);
        spyOn(testDynamicControl, `validate`);

        dynamicControl.validate();

        expect(testDynamicControl.validate).toHaveBeenCalledTimes(1);
    });
});

function stubValidator(validity: Validity, message: string): DynamicControlValidator {
    return (dynamicControl: DynamicControl<any>): DynamicControlValidatorResult => {
        return new DynamicControlValidatorResult(validity, message);
    }; 
}

function stubAsyncValidator(validity: Validity, message: string): DynamicControlAsyncValidator {
    return new DynamicControlAsyncValidator(
        (dynamicControl: DynamicControl<any>, ): DynamicControlValidatorResult => {
            return new DynamicControlValidatorResult(validity, message);
        },
        null);
}
