import { Injectable } from '@angular/core';
import { Http, Response, URLSearchParams } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { environment } from '../../../environments/environment';
import { DynamicForm } from './dynamic-form/dynamic-form';
import { DynamicControl } from './dynamic-control/dynamic-control';

/**
 * Wraps Http requests and response processing for the dynamic forms system.
 */
@Injectable()
export class DynamicFormsService {
    private _dynamicFormsUrl = `${environment.apiUrl}DynamicForms/GetDynamicForm`;

    constructor(private _http: Http) { }

    /**
     * Sends get http request asynchronously. Maps function that constructs a dynamic form from retrieved data.
     *
     * Returns
     * - Observable<DynamicForm>
     */
    getDynamicForm(formModelName: string): Observable<DynamicForm> {
        let urlSearchParams = new URLSearchParams();
        urlSearchParams.set(`formModelName`, formModelName);

        return this._http.
            get(this._dynamicFormsUrl, { search: urlSearchParams }).
            map(this.createDynamicFormFromData);
    }

    /**
     * Converts a Response into a DynamicForm
     */
    private createDynamicFormFromData(response: Response): DynamicForm {
        let body = response.json();
        let dynamicControls: DynamicControl<any>[] = [];

        for (let dynamicControlData of body.dynamicControlDatas) {
            dynamicControls.push(new DynamicControl<any>(dynamicControlData));
        }

        return new DynamicForm(dynamicControls.sort((dynamicControl1, dynamicControl2) => dynamicControl1.order - dynamicControl2.order));
    }
}
