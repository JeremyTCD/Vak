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

describe(`DynamicControl`, () => {
    beforeEach(() => {
        dynamicControl = new DynamicControl({});
    });

    describe(`onInput`, () => {
        let stubInputEvent: StubDomEvent;

        beforeEach(() => {
            stubInputEvent = new StubDomEvent(new StubDomElement(testValue));
        });

        it(`Sets value, sets dirty to true and calls validate if blurred is truthy`, () => {
            dynamicControl.blurred = true;
            spyOn(dynamicControl, `validate`);

            dynamicControl.onInput(stubInputEvent);

            expect(dynamicControl.value).toBe(testValue);
            expect(dynamicControl.dirty).toBe(true);
            expect(dynamicControl.validate).toHaveBeenCalledTimes(1);
        });

        it(`Calls parent.validate if validity changes`, () => {
            dynamicControl.blurred = true;
            spyOn(dynamicControl, `validate`).and.callFake(() => {
                dynamicControl.validity = Validity.valid;
            });
            testDynamicForm = new DynamicForm(null, null);
            testDynamicForm.submitAttempted = true;
            spyOn(testDynamicForm, `validate`);
            dynamicControl.parent = testDynamicForm;

            dynamicControl.onInput(stubInputEvent);

            expect(testDynamicForm.validate).toHaveBeenCalledTimes(1);
        });

        it(`Does not call parent.validate if validity does not change`, () => {
            dynamicControl.blurred = true;
            spyOn(dynamicControl, `validate`);
            testDynamicForm = new DynamicForm(null, null);
            testDynamicForm.submitAttempted = true;
            spyOn(testDynamicForm, `validate`);
            dynamicControl.parent = testDynamicForm;

            dynamicControl.onInput(stubInputEvent);

            expect(testDynamicForm.validate).not.toHaveBeenCalled();
        });

        it(`Sets value, sets dirty to true but does not call validate if blurred is falsey`, () => {
            spyOn(dynamicControl, `validate`);

            dynamicControl.onInput(stubInputEvent);

            expect(dynamicControl.value).toBe(testValue);
            expect(dynamicControl.dirty).toBe(true);
            expect(dynamicControl.validate).not.toHaveBeenCalled();
        });
    });

    describe(`onBlur`, () => {
        let stubInputEvent: StubDomEvent;

        beforeEach(() => {
            stubInputEvent = new StubDomEvent(new StubDomElement(testValue));
        });

        it(`Sets blurred to true and calls validate if dirty is truthy and blurred is falsey`, () => {
            dynamicControl.dirty = true;
            spyOn(dynamicControl, `validate`);

            dynamicControl.onBlur(stubInputEvent);

            expect(dynamicControl.blurred).toBe(true);
            expect(dynamicControl.validate).toHaveBeenCalledTimes(1);
        });

        it(`Calls parent.validate if validity changes`, () => {
            dynamicControl.dirty = true;
            spyOn(dynamicControl, `validate`).and.callFake(() => {
                dynamicControl.validity = Validity.valid;
            });
            testDynamicForm = new DynamicForm(null, null);
            testDynamicForm.submitAttempted = true;
            spyOn(testDynamicForm, `validate`);
            dynamicControl.parent = testDynamicForm;

            dynamicControl.onBlur(stubInputEvent);

            expect(testDynamicForm.validate).toHaveBeenCalledTimes(1);
        });

        it(`Does not call parent.validate if validity does not change`, () => {
            dynamicControl.dirty = true;
            spyOn(dynamicControl, `validate`);
            testDynamicForm = new DynamicForm(null, null);
            testDynamicForm.submitAttempted = true;
            spyOn(testDynamicForm, `validate`);
            dynamicControl.parent = testDynamicForm;

            dynamicControl.onBlur(stubInputEvent);

            expect(testDynamicForm.validate).not.toHaveBeenCalled();
        });

        it(`Does nothing if dirty is falsey`, () => {
            spyOn(dynamicControl, `validate`);

            dynamicControl.onBlur(stubInputEvent);

            expect(dynamicControl.blurred).not.toBeDefined();
            expect(dynamicControl.validate).not.toHaveBeenCalled();
        });

        it(`Does nothing if blurred is truthy`, () => {
            dynamicControl.blurred = true;
            spyOn(dynamicControl, `validate`);

            dynamicControl.onBlur(stubInputEvent);

            expect(dynamicControl.validate).not.toHaveBeenCalled();
        });
    });

    it(`setupContext sets parent and registers with provider siblings`, () => {
        dynamicControl.providerSiblingsNames.push(testDynamicControlName);
        let testDynamicControl = new DynamicControl({ name: testDynamicControlName });
        testDynamicForm = new DynamicForm([testDynamicControl], null);
        spyOn(testDynamicForm, `getDynamicControl`).and.returnValue(testDynamicControl);

        dynamicControl.setupContext(testDynamicForm);

        expect(testDynamicForm.getDynamicControl).toHaveBeenCalledTimes(1);
        expect(dynamicControl.parent).toBe(testDynamicForm);
        expect(testDynamicControl.dependentSiblings[0]).toBe(dynamicControl);
    });

    it(`setupContext sets parent but throws error if sibling does not exist `, () => {
        dynamicControl.providerSiblingsNames.push(testDynamicControlName);
        testDynamicForm = new DynamicForm([], null);
        spyOn(testDynamicForm, `getDynamicControl`).and.returnValue(null);

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
        dynamicControl.asyncValidator = new DynamicControlAsyncValidator(null, null, null);
        spyOn(dynamicControl.asyncValidator, `validate`).and.returnValue(new DynamicControlValidatorResult(Validity.pending, testValidatorMessage));

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