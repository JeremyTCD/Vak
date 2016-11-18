import { Check } from './check';

describe('isBlank', () => {
    it('returns true if argument is undefined', () => {
        expect(Check.isBlank(undefined)).toBe(true);
    });

    it('returns true if argument is null', () => {
        expect(Check.isBlank(null)).toBe(true);
    });

    it('returns false if argument is not null or undefined', () => {
        expect(Check.isBlank(false)).toBe(false);
        expect(Check.isBlank(0)).toBe(false);
        expect(Check.isBlank({})).toBe(false);
        expect(Check.isBlank([])).toBe(false);
        expect(Check.isBlank('test')).toBe(false);
    });
});

describe('isString', () => {
    it('returns true if argument is a string', () => {
        expect(Check.isString('test')).toBe(true);
    });

    it('returns false if argument is not a string', () => {
        expect(Check.isString(false)).toBe(false);
        expect(Check.isString(0)).toBe(false);
        expect(Check.isString({})).toBe(false);
        expect(Check.isString([])).toBe(false);
        expect(Check.isString(null)).toBe(false);
        expect(Check.isString(undefined)).toBe(false);
    });
});

describe('isEmptyString', () => {
    it('returns true if argument is an empty string', () => {
        expect(Check.isEmptyString('')).toBe(true);
    });

    it('returns false if argument is a non-empty string', () => {
        expect(Check.isEmptyString('test')).toBe(false);
        expect(Check.isEmptyString(null)).toBe(false);
        expect(Check.isEmptyString(undefined)).toBe(false);
    });
});

describe('isValue', () => {
    it('returns false if argument is undefined', () => {
        expect(Check.isValue(undefined)).toBe(false);
    });

    it('returns false if argument is null', () => {
        expect(Check.isValue(null)).toBe(false);
    });

    it('returns false if argument is an empty string', () => {
        expect(Check.isValue('')).toBe(false);
    });

    it('returns true if argument is not null, undefined, or an empty string', () => {
        expect(Check.isValue(false)).toBe(true);
        expect(Check.isValue(0)).toBe(true);
        expect(Check.isValue({})).toBe(true);
        expect(Check.isValue([])).toBe(true);
        expect(Check.isValue('test')).toBe(true);
    });
});

describe('isDigit', () => {
    it('returns true if argument is a digit', () => {
        expect(Check.isDigit('0')).toBe(true);
        expect(Check.isDigit('1')).toBe(true);
        expect(Check.isDigit('2')).toBe(true);
        expect(Check.isDigit('3')).toBe(true);
        expect(Check.isDigit('4')).toBe(true);
        expect(Check.isDigit('5')).toBe(true);
        expect(Check.isDigit('6')).toBe(true);
        expect(Check.isDigit('7')).toBe(true);
        expect(Check.isDigit('8')).toBe(true);
        expect(Check.isDigit('9')).toBe(true);
    });

    it('returns false if argument is not a digit', () => {
        expect(Check.isDigit('test')).toBe(false);
        expect(Check.isDigit('a')).toBe(false);
        expect(Check.isDigit('11')).toBe(false);
        expect(Check.isDigit('-1')).toBe(false);
        expect(Check.isDigit(null)).toBe(false);
        expect(Check.isDigit(undefined)).toBe(false);
    });
});

describe('isLower', () => {
    it('returns true if argument is a lower case alphabet', () => {
        expect(Check.isLower('a')).toBe(true);
        expect(Check.isLower('b')).toBe(true);
        expect(Check.isLower('c')).toBe(true);
        expect(Check.isLower('d')).toBe(true);
        expect(Check.isLower('e')).toBe(true);
        expect(Check.isLower('f')).toBe(true);
        expect(Check.isLower('g')).toBe(true);
        expect(Check.isLower('h')).toBe(true);
        expect(Check.isLower('i')).toBe(true);
        expect(Check.isLower('j')).toBe(true);
        expect(Check.isLower('k')).toBe(true);
        expect(Check.isLower('l')).toBe(true);
        expect(Check.isLower('m')).toBe(true);
        expect(Check.isLower('n')).toBe(true);
        expect(Check.isLower('o')).toBe(true);
        expect(Check.isLower('p')).toBe(true);
        expect(Check.isLower('q')).toBe(true);
        expect(Check.isLower('r')).toBe(true);
        expect(Check.isLower('s')).toBe(true);
        expect(Check.isLower('t')).toBe(true);
        expect(Check.isLower('u')).toBe(true);
        expect(Check.isLower('v')).toBe(true);
        expect(Check.isLower('w')).toBe(true);
        expect(Check.isLower('x')).toBe(true);
        expect(Check.isLower('y')).toBe(true);
        expect(Check.isLower('z')).toBe(true);
    });

    it('returns false if argument is not a lower case alphabet', () => {
        expect(Check.isLower('A')).toBe(false);
        expect(Check.isLower('B')).toBe(false);
        expect(Check.isLower('Z')).toBe(false);
        expect(Check.isLower('aa')).toBe(false);
        expect(Check.isLower('1')).toBe(false);
        expect(Check.isLower('@')).toBe(false);
        expect(Check.isLower(null)).toBe(false);
        expect(Check.isLower(undefined)).toBe(false);
    });
});

describe('isUpper', () => {
    it('returns true if argument is an upper case alphabet', () => {
        expect(Check.isUpper('A')).toBe(true);
        expect(Check.isUpper('B')).toBe(true);
        expect(Check.isUpper('C')).toBe(true);
        expect(Check.isUpper('D')).toBe(true);
        expect(Check.isUpper('E')).toBe(true);
        expect(Check.isUpper('F')).toBe(true);
        expect(Check.isUpper('G')).toBe(true);
        expect(Check.isUpper('H')).toBe(true);
        expect(Check.isUpper('I')).toBe(true);
        expect(Check.isUpper('J')).toBe(true);
        expect(Check.isUpper('K')).toBe(true);
        expect(Check.isUpper('L')).toBe(true);
        expect(Check.isUpper('M')).toBe(true);
        expect(Check.isUpper('N')).toBe(true);
        expect(Check.isUpper('O')).toBe(true);
        expect(Check.isUpper('P')).toBe(true);
        expect(Check.isUpper('Q')).toBe(true);
        expect(Check.isUpper('R')).toBe(true);
        expect(Check.isUpper('S')).toBe(true);
        expect(Check.isUpper('T')).toBe(true);
        expect(Check.isUpper('U')).toBe(true);
        expect(Check.isUpper('V')).toBe(true);
        expect(Check.isUpper('W')).toBe(true);
        expect(Check.isUpper('X')).toBe(true);
        expect(Check.isUpper('Y')).toBe(true);
        expect(Check.isUpper('Z')).toBe(true);
    });

    it('returns false if argument is not an upper case alphabet', () => {
        expect(Check.isUpper('a')).toBe(false);
        expect(Check.isUpper('b')).toBe(false);
        expect(Check.isUpper('z')).toBe(false);
        expect(Check.isUpper('AA')).toBe(false);
        expect(Check.isUpper('1')).toBe(false);
        expect(Check.isUpper('@')).toBe(false);
        expect(Check.isUpper(null)).toBe(false);
        expect(Check.isUpper(undefined)).toBe(false);
    });
});

describe('isNonAlphaNumeric', () => {
    it('returns true if argument is a non alpha numeric character', () => {
        expect(Check.isNonAlphaNumeric('(')).toBe(true);
        expect(Check.isNonAlphaNumeric(')')).toBe(true);
        expect(Check.isNonAlphaNumeric('`')).toBe(true);
        expect(Check.isNonAlphaNumeric('~')).toBe(true);
        expect(Check.isNonAlphaNumeric('!')).toBe(true);
        expect(Check.isNonAlphaNumeric('@')).toBe(true);
        expect(Check.isNonAlphaNumeric('#')).toBe(true);
        expect(Check.isNonAlphaNumeric('$')).toBe(true);
        expect(Check.isNonAlphaNumeric('%')).toBe(true);
        expect(Check.isNonAlphaNumeric('^')).toBe(true);
        expect(Check.isNonAlphaNumeric('&')).toBe(true);
        expect(Check.isNonAlphaNumeric('*')).toBe(true);
        expect(Check.isNonAlphaNumeric('-')).toBe(true);
        expect(Check.isNonAlphaNumeric('+')).toBe(true);
        expect(Check.isNonAlphaNumeric('=')).toBe(true);
        expect(Check.isNonAlphaNumeric('|')).toBe(true);
        expect(Check.isNonAlphaNumeric('\\')).toBe(true);
        expect(Check.isNonAlphaNumeric('{')).toBe(true);
        expect(Check.isNonAlphaNumeric('}')).toBe(true);
        expect(Check.isNonAlphaNumeric('[')).toBe(true);
        expect(Check.isNonAlphaNumeric(']')).toBe(true);
        expect(Check.isNonAlphaNumeric(':')).toBe(true);
        expect(Check.isNonAlphaNumeric(';')).toBe(true);
        expect(Check.isNonAlphaNumeric('"')).toBe(true);
        expect(Check.isNonAlphaNumeric('\'')).toBe(true);
        expect(Check.isNonAlphaNumeric('<')).toBe(true);
        expect(Check.isNonAlphaNumeric('>')).toBe(true);
        expect(Check.isNonAlphaNumeric(',')).toBe(true);
        expect(Check.isNonAlphaNumeric('.')).toBe(true);
        expect(Check.isNonAlphaNumeric('?')).toBe(true);
        expect(Check.isNonAlphaNumeric('/')).toBe(true);
        expect(Check.isNonAlphaNumeric('_')).toBe(true);
    });

    it('returns false if argument is not a non alpha numeric character', () => {
        expect(Check.isNonAlphaNumeric('a')).toBe(false);
        expect(Check.isNonAlphaNumeric('A')).toBe(false);
        expect(Check.isNonAlphaNumeric('1')).toBe(false);
        expect(Check.isNonAlphaNumeric('@@')).toBe(false);
        expect(Check.isNonAlphaNumeric(null)).toBe(false);
        expect(Check.isNonAlphaNumeric(undefined)).toBe(false);
    });
});

describe(`isObject`, () => {
    it(`returns true if argument is a non-null object`, () => {
        expect(Check.isObject({})).toBe(true);
        expect(Check.isObject([])).toBe(true);
    });

    it(`returns false if argument is not a non-null object`, () => {
        expect(Check.isObject(0)).toBe(false);
        expect(Check.isObject(null)).toBe(false);
        expect(Check.isObject(`test`)).toBe(false);
        expect(Check.isObject(undefined)).toBe(false);
    });
});
