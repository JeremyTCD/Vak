import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { DynamicFormsService } from '../dynamic-forms.service';
import { DynamicForm } from '../dynamic-form/dynamic-form';

@Injectable()
export class DynamicFormGuard implements Resolve<DynamicForm> {
    constructor(private _dynamicFormsService: DynamicFormsService) { }

    resolve(route: ActivatedRouteSnapshot): Observable<DynamicForm>{
        return this._dynamicFormsService
            .getDynamicForm(route.data[`formModelName`]);
    };
}
