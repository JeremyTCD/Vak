import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { SignUpComponent } from './sign-up.component';
import { DynamicFormGuard } from 'app/shared/dynamic-forms/dynamic-form/dynamic-form.guard';

const signUpRoutes: Routes = [
    {
        path: '',
        component: SignUpComponent,
        data: {
            requestModelName: SignUpComponent.requestModelName,
            formSubmitRelativeUrl: SignUpComponent.formSubmitRelativeUrl
        },
        resolve: {
            dynamicForm: DynamicFormGuard
        }
    }
];

export const SignUpRouting: ModuleWithProviders = RouterModule.forChild(signUpRoutes);
