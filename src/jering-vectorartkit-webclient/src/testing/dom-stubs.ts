export class StubDomEvent {

    constructor(public target?: StubDomElement) {
    }

    preventDefault(): null {
        return null
    }
}

export class StubDomElement {
    constructor(public value?: any) {
    }

    checked: boolean;
} 
