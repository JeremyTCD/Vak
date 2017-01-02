import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { HttpModule } from '@angular/http';

import { AppComponent } from './app.component';
import { AppRouting } from './app.routing';

import { CookieService } from 'app/shared/cookie.service';
import { UserService } from 'app/shared/user.service';
import { StorageService } from 'app/shared/storage.service';
import { HttpService } from 'app/shared/http.service';
import { ErrorHandlerService } from 'app/shared/error-handler.service';
import { AuthGuard } from 'app/shared/auth.guard';
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
    providers: [CookieService,
        UserService,
        StorageService,
        HttpService,
        ErrorHandlerService,
        AuthGuard],
    bootstrap: [AppComponent]
})
export class AppModule { }
