import { DynamicControl } from '../dynamic-control/dynamic-control';
import { DynamicForm } from './dynamic-form';
import { Validity } from '../validity';
import { StubDomEvent, StubDomElement } from '../../../../testing/dom-stubs';

let dynamicForm: DynamicForm;
let testDynamicControl: DynamicControl;
let testDynamicForm: DynamicForm;
let testMessage = `testMessage`;
let testValue = `testValue`;
let testName = `testName`;

describe('DynamicForm', () => {
    beforeEach(() => {
        testDynamicControl = new DynamicControl({});
        dynamicForm = new DynamicForm([testDynamicControl], testMessage, null);
    });

    describe(`onSubmit`, () => {
        it(`Sets submitAttempted to true, sets dirty and blurred to true for all child DynamicControls and calls validate`, () => {
            spyOn(dynamicForm, `validate`);

            dynamicForm.onSubmit();

            expect(dynamicForm.submitAttempted).toBe(true);
            expect(dynamicForm.dynamicControls[0].dirty).toBe(true);
            expect(dynamicForm.dynamicControls[0].blurred).toBe(true);
            expect(dynamicForm.validate).toHaveBeenCalledTimes(1);
        });

        it(`Returns true if validity is Validity.valid`, () => {
            spyOn(dynamicForm, `validate`).and.callFake(() => {
                dynamicForm.validity = Validity.valid;
            });

            expect(dynamicForm.onSubmit()).toBe(true);
        });

        it(`Returns false if validity is Validity.invalid`, () => {
            spyOn(dynamicForm, `validate`).and.callFake(() => {
                dynamicForm.validity = Validity.invalid;
            });

            expect(dynamicForm.onSubmit()).toBe(false);
        });

        it(`Returns false if validity is Validity.invalid`, () => {
            spyOn(dynamicForm, `validate`).and.callFake(() => {
                dynamicForm.validity = Validity.pending;
            });

            expect(dynamicForm.onSubmit()).toBe(false);
        });
    });

    it(`value returns map of child DynamicControl name-value pairs`, () => {
        testDynamicControl.name = testName;
        testDynamicControl.value = testValue;

        expect(dynamicForm.value).toEqual({ testName: testValue });
    });

    it(`getDynamicControl returns DynamicControl with specified name`, () => {
        testDynamicControl.name = testName;

        expect(dynamicForm.getDynamicControl(testName)).toBe(testDynamicControl);
    });

    describe(`validate`, () => {
        it(`Calls DynamicControl.validate on child DynamicControls that have validate === undefined`, () => {
            spyOn(testDynamicControl, `validate`);

            dynamicForm.validate();

            expect(testDynamicControl.validate).toHaveBeenCalledTimes(1);
        });

        it(`Does not call DynamicControl.validate on child DynamicControls that have validate !== undefined`, () => {
            testDynamicControl.validity = Validity.valid;
            spyOn(testDynamicControl, `validate`);

            dynamicForm.validate();

            expect(testDynamicControl.validate).not.toHaveBeenCalled();
        });

        it(`Sets validity to Validity.invalid and pushes message if any child DynamicControl has validity === Validity.invalid 
            or validity === Validity.pending`, () => {
                testDynamicControl.validity = Validity.invalid;

                dynamicForm.validate();

                expect(dynamicForm.validity).toBe(Validity.invalid);
                expect(dynamicForm.messages[0]).toBe(testMessage);
            });

        it(`Sets validity to Validity.valid and clears messages if all child DynamicControls have validity === Validity.valid`, () => {
            testDynamicControl.validity = Validity.valid;
            dynamicForm.messages[0] = testMessage;

            dynamicForm.validate();

            expect(dynamicForm.validity).toBe(Validity.valid);
            expect(dynamicForm.messages.length).toBe(0);
        });
    });

    it(`setupContext calls DynamicControl.setupContext for each child DynamicControl`, () => {
        spyOn(testDynamicControl, `setupContext`);

        dynamicForm.setupContext();

        expect(testDynamicControl.setupContext).toHaveBeenCalledTimes(1);
    });

    it(`dispose calls DynamicControl.dispose for each child DynamicControl`, () => {
        spyOn(testDynamicControl, `dispose`);

        dynamicForm.dispose();

        expect(testDynamicControl.dispose).toHaveBeenCalledTimes(1);
    });
});
