import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { LogInComponent } from './log-in.component';
import { DynamicFormsResolve } from '../shared/dynamic-forms/dynamic-forms-resolve';

const logInRoutes: Routes = [
    {
        path: '',
        component: LogInComponent,
        data: {
            formModelName: LogInComponent.formModelName,
            formSubmitRelativeUrl: LogInComponent.formSubmitRelativeUrl
        },
        resolve: {
            dynamicForm: DynamicFormsResolve
        }
    }
];

export const LogInRouting: ModuleWithProviders = RouterModule.forChild(logInRoutes);
