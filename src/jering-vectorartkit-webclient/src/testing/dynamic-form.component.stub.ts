import { Component, Output, EventEmitter } from '@angular/core';

import { DynamicForm } from 'app/shared/dynamic-forms/dynamic-form/dynamic-form';

@Component({
    selector: 'dynamic-form',
    template: '<a (click)="submitSuccess.emit();submitError.emit();"></a>'
})
export class StubDynamicFormComponent {
    @Output() submitSuccess = new EventEmitter<any>();
    @Output() submitError = new EventEmitter<any>();
    dynamicForm: DynamicForm = new DynamicForm([], null, null);
}
