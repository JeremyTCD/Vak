import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { SignUpAppComponent } from './sign-up-app.component';

@NgModule({
    imports: [
        BrowserModule,
        FormsModule,
        HttpModule
    ],
    declarations: [
        SignUpAppComponent
    ],
    bootstrap: [SignUpAppComponent]
})
export class AppModule { }