import { Component, Input, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { Renderer } from '@angular/core/index';

import { DynamicControl } from './dynamic-control';

/**
 * Component for a single DynamicControl
 */
@Component({
    selector: 'dynamic-control',
    templateUrl: 'dynamic-control.component.html'
})
export class DynamicControlComponent implements AfterViewInit {
    @Input() dynamicControl: DynamicControl<any>;
    @ViewChild(`control`) control: ElementRef;

    constructor(private _renderer: Renderer) { }

    ngAfterViewInit(): void {
        for (let key of Object.keys(this.dynamicControl.properties)) {
            this._renderer.setElementProperty(this.control.nativeElement, key, this.dynamicControl.properties[key]);
        }
    }
}
