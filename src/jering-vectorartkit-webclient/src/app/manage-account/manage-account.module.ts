import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ManageAccountComponent } from './manage-account.component';
import { ManageAccountRouting } from './manage-account.routing';
import { ManageAccountGuard } from './manage-account.guard';

@NgModule({
    imports: [
        ManageAccountRouting,
        CommonModule
    ],
    providers: [
        ManageAccountGuard
    ],
    declarations: [
        ManageAccountComponent
    ]
})
export class ManageAccountModule { }
