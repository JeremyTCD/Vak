import { Validity } from '../validity';

export class DynamicControlValidatorResult {
    constructor(public validity: Validity, public message?: string) {
    }
}