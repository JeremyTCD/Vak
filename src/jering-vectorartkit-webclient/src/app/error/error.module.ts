import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ErrorComponent } from './error.component';
import { ErrorRouting } from './error.routing';

@NgModule({
    imports: [ErrorRouting, CommonModule],
    declarations: [ErrorComponent]
})
export class ErrorModule { }
