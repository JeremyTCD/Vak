import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { SignUpComponent } from './sign-up.component';

const signUpRoutes: Routes = [
    { path: 'signup', component: SignUpComponent }
];

export const SignUpRouting: ModuleWithProviders = RouterModule.forChild(signUpRoutes);
