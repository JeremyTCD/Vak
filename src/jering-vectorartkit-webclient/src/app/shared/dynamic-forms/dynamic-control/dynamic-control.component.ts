import { Component, Input, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { Renderer } from '@angular/core/index';

import { DynamicControl } from './dynamic-control';
import { Validity } from '../validity';

/**
 * Component for a single DynamicControl
 */
@Component({
    selector: 'dynamic-control',
    templateUrl: 'dynamic-control.component.html'
})
export class DynamicControlComponent implements AfterViewInit {
    @Input() dynamicControl: DynamicControl;
    @ViewChild(`control`) control: ElementRef;

    constructor(private _renderer: Renderer) { }

    ngAfterViewInit(): void {
        // Set html properties
        for (let key of Object.keys(this.dynamicControl.properties)) {
            this._renderer.setElementProperty(this.control.nativeElement, key, this.dynamicControl.properties[key]);
        }
        this._renderer.setElementProperty(this.control.nativeElement, `id`, this.dynamicControl.name);

        // Add listeners
        let type = this.dynamicControl.properties[`type`];
        if (this.dynamicControl.tagName === `input`) {
            if (type === `email` ||
                type === `password` ||
                type === `text`) {
                // onInput called with no this context?
                this._renderer.listen(this.control.nativeElement, `input`, (event) => this.dynamicControl.onInput(event));
                this._renderer.listen(this.control.nativeElement, `blur`, (event) => this.dynamicControl.onBlur(event));
            } else if (type === `checkbox`) {
                this._renderer.listen(this.control.nativeElement, `change`, (event) => this.dynamicControl.onChange(event));
            }
        } else if (this.dynamicControl.tagName === `select`) {

        }
    }

    getClasses(): string[] {
        return [Validity[this.dynamicControl.validity]];
    }
}
