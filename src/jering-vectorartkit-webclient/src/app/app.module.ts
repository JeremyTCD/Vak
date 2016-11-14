import { NgModule } from '@angular/core';
import { AppComponent } from './app.component';
import { VakAppModule } from './vak-app/vak-app.module';
//import { AppRouting } from './app.routing';

@NgModule({
    imports: [
        VakAppModule
        //AppRouting
    ],
    declarations: [AppComponent],
    bootstrap: [AppComponent]
})
export class AppModule { }
