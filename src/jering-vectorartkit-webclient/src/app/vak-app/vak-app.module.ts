import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';

import { VakAppRouting } from './vak-app.routing';
import { VakAppComponent } from './vak-app.component';
import { SignUpModule } from '../sign-up/sign-up.module';
import { HomeModule } from '../home/home.module';
import { ErrorModule } from '../error/error.module';
import { UserService } from '../shared/user.service';

@NgModule({
    imports: [
        BrowserModule,
        CommonModule,
        SignUpModule,
        HomeModule,
        ErrorModule,
        VakAppRouting
    ],
    declarations: [VakAppComponent],
    exports: [VakAppComponent],
    providers: [UserService]
})
export class VakAppModule { }
