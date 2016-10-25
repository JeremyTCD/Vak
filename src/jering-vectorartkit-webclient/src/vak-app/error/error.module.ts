import { NgModule } from '@angular/core';

import { ErrorComponent } from './error.component';
import { ErrorRouting } from './error.routing';

@NgModule({
    imports: [
        ErrorRouting
    ],
    declarations: [
        ErrorComponent
    ]
})
export class ErrorModule { }
