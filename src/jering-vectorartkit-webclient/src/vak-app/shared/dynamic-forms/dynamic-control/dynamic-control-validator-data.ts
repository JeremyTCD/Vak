﻿/**
 * Data used to defined a DynamicControlValidator
 */
export class DynamicControlValidatorData {
    name: string;
    errorMessage: string;
    options: { [key: string]: string };

    constructor(options: {
        name: string,
        errorMessage: string,
        options: { [key: string]: string }
    }) {
        this.name = options.name;
        this.errorMessage = options.errorMessage;
        this.options = options.options;
    }
}