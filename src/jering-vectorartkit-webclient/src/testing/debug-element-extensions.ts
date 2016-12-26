import { DebugElement, Type } from '@angular/core';

export class DebugElementExtensions {
    // Returns true if debugElement has a descendant with text as its textContent
    static hasDescendantWithInnerHtml(debugElement: DebugElement, text: string): boolean {
        return debugElement.
            queryAll(element => DebugElementExtensions.hasInnerHtml(element, text)).length > 0;
    }

    // Returns first descendant with text as its textContent
    static getDescendantWithInnerHtml(debugElement: DebugElement, text: string): DebugElement {
        return debugElement.
            query(element => DebugElementExtensions.hasInnerHtml(element, text));
    }
    // Returns true if debugElement's innerHtml is text. 
    static hasInnerHtml(debugElement: DebugElement, text: string): boolean {
        return debugElement.nativeElement.innerHTML.trim() === text;
    }
}