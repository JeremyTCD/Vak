import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';

import { AppComponent } from './app.component';
import { AppRouting } from './app.routing';

import { SignUpModule } from './sign-up/sign-up.module';
import { HomeModule } from './home/home.module';
import { ErrorModule } from './error/error.module';
import { UserService } from './shared/user.service';
import { StorageService } from './shared/storage.service';
import { HttpService } from './shared/http.service';

@NgModule({
    imports: [
        BrowserModule,
        CommonModule,
        SignUpModule,
        HomeModule,
        ErrorModule,
        AppRouting
    ],
    declarations: [AppComponent],
    providers: [UserService,
        StorageService,
        HttpService],
    bootstrap: [AppComponent]
})
export class AppModule { }
