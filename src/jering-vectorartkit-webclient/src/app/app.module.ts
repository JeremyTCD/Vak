import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { HttpModule } from '@angular/http';

import { AppComponent } from './app.component';
import { AppRouting } from './app.routing';

import { UserService } from './shared/user.service';
import { StorageService } from './shared/storage.service';
import { HttpService } from './shared/http.service';
import { ErrorHandlerService } from './shared/error-handler.service';
import { HomeModule } from './home/home.module';

@NgModule({
    imports: [
        AppRouting,
        HttpModule,
        BrowserModule,
        CommonModule,
        HomeModule
    ],
    declarations: [AppComponent],
    providers: [UserService,
        StorageService,
        HttpService,
        ErrorHandlerService],
    bootstrap: [AppComponent]
})
export class AppModule { }
