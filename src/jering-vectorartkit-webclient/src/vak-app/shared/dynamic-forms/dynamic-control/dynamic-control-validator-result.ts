import { Validity } from '../validity';

export class DynamicControlValidatorResult {
    constructor(public validity: Validity, public errorMessage?: string) {
    }
}