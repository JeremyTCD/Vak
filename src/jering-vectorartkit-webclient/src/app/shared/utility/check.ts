export class Check {
    private static nonAlphaNumericChars = ['(', ')', '`', '~', '!', '@', '#', '$', '%', '^', '&', '*', '-', '+',
        '=', '|', '\\', '{', '}', '[', ']', ':', ';', '"',
        '\'', '<', '>', ',', '.', '?', '/', '_'];

    /**
     * If obj is null or undefined, returns true. Otherwise, returns false.
     */
    static isBlank(obj: any): boolean {
        return obj === undefined || obj === null;
    }

    /**
     * If obj is a string, returns true and marks obj as a string. Otherwise, returns false.
     */
    static isString(obj: any): obj is string {
        return typeof obj === 'string';

    }

    /**
     * If str is an empty string, '', returns true. Otherwise, returns false.
     */
    static isEmptyString(str: string): boolean {
        return this.isString(str) && str.trim().length === 0;
    }

    /**
     * If obj is not null, undefined or an empty string, returns true. Otherwise, returns false.
     */
    static isValue(obj: any): boolean {
        return !this.isBlank(obj) && !this.isEmptyString(obj as string);
    }

    /**
     * If char is a digit, returns true. Otherwise, returns false. 
     */
    static isDigit(char: string): boolean {
        // String comparison is done by comparing lexicographic order, meaning that
        // '10' >= '0' and '10' <= '9' are both true. Therefore char's length must be 
        // fixed to 1.
        return !this.isBlank(char) && char.trim().length === 1 && char >= '0' && char <= '9';
    }

    /**
     * If char is a lower case alphabet, returns true. Otherwise, returns false. 
     */
    static isLower(char: string): boolean {
        return !this.isBlank(char) && char.trim().length === 1 && char >= 'a' && char <= 'z';
    }

    /**
     * If char is an upper case alphabet, returns true. Otherwise, returns false. 
     */
    static isUpper(char: string): boolean {
        return !this.isBlank(char) && char.trim().length === 1 && char >= 'A' && char <= 'Z';
    }

    /**
     * If char is a non alpha numeric character, returns true. Otherwise, returns false.
     */
    static isNonAlphaNumeric(char: string): boolean {
        return !this.isBlank(char) && char.trim().length === 1 && this.nonAlphaNumericChars.indexOf(char) > -1;
    }
}
