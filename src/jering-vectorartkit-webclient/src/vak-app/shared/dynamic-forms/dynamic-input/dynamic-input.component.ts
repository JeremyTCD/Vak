import { Component, Input, AfterViewInit, ViewChild, Renderer} from '@angular/core';
import { ElementRef } from '@angular/core/index';

import { DynamicInput } from './dynamic-input';

@Component({
    selector: 'dynamic-input',
    templateUrl: 'dynamic-input.component.html'
})
export class DynamicInputComponent implements AfterViewInit {
    @Input() dynamicInput: DynamicInput<any>;
    @ViewChild('control') control: ElementRef;

    constructor(private _renderer: Renderer) {}

    ngAfterViewInit(): void{
        if (this.dynamicInput.tagName === 'input') {
            this._renderer.listen(this.control.nativeElement, 'input', (event) => { this.onInput(event) });
            this._renderer.listen(this.control.nativeElement, 'blur', (event) => { this.onBlur(event) });
        } else if (this.dynamicInput.tagName === 'select') {
            this._renderer.listen(this.control.nativeElement, 'change', (event) => { this.onChange(event) });
        }
    }

    /**
     * For text input and textarea, called on every keystroke.
     */
    onInput(event: any): void {
        this.dynamicInput.value = event.target.value;
        this.dynamicInput.dirty = true;
        event.stopPropagation();
        this.dynamicInput.parent.validate();
    }

    /**
     * Called when target loses focus.
     */
    onBlur(event: any): void {
        this.dynamicInput.blurred = true;
        event.stopPropagation();
        this.dynamicInput.parent.validate();
    }

    /**
     * For select and checkbox input, called when value changes.
     */
    onChange(event: any): void {
        this.dynamicInput.value = event.target.value;
        this.dynamicInput.dirty = true;
        this.dynamicInput.blurred = true;
        event.stopPropagation();
        this.dynamicInput.parent.validate();
    }
}
