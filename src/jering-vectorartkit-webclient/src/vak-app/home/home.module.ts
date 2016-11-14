import { NgModule } from '@angular/core';

import { HomeComponent } from './home.component';
import { HomeRouting } from './home.routing';

@NgModule({
    imports: [
        HomeRouting
    ],
    declarations: [
        HomeComponent
    ]
})
export class HomeModule { }
