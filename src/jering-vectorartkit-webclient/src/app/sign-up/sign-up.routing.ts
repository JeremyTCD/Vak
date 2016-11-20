import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { SignUpComponent } from './sign-up.component';
import { DynamicFormsResolve } from '../shared/dynamic-forms/dynamic-forms-resolve';

const signUpRoutes: Routes = [
    {
        path: '',
        component: SignUpComponent,
        data: {
            formModelName: SignUpComponent.formModelName,
            formSubmitRelativeUrl: SignUpComponent.formSubmitRelativeUrl
        },
        resolve: {
            dynamicForm: DynamicFormsResolve
        }
    }
];

export const SignUpRouting: ModuleWithProviders = RouterModule.forChild(signUpRoutes);
