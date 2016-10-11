export function isBlank(obj: any): boolean {
    return obj === undefined || obj === null;
}

export function isString(obj: any): obj is string {
    return typeof obj === 'string';

}

export function isEmptyString(str: string): boolean {
    return str.trim().length == 0;
}

export function isValue(obj: any): boolean {
    return !isBlank(obj) && (!isString(obj) || !isEmptyString(obj as string)); 
}

export function isDigit(char: string): boolean {
    return char >= '0' && char <= '9';
}