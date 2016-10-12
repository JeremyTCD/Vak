import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';

import { VakAppComponent } from './vak-app.component';
import { VakAppRouting } from './vak-app.routing';

import { SignUpModule } from '../sign-up/sign-up.module';

@NgModule({
    imports: [
        BrowserModule,
        CommonModule,
        SignUpModule,
        VakAppRouting
    ],
    declarations: [VakAppComponent],
    bootstrap: [VakAppComponent]
})  
export class VakAppModule { }