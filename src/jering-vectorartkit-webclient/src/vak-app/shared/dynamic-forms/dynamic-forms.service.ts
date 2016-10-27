import { Injectable } from '@angular/core';
import { Http, Response, URLSearchParams, Headers, RequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { ErrorObservable } from 'rxjs/observable/ErrorObservable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

import { environment } from '../../../environments/environment';
import { DynamicForm } from './dynamic-form/dynamic-form';
import { DynamicControl } from './dynamic-control/dynamic-control';
import { Validity } from './validity';

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
        let requestOptions = new RequestOptions({withCredentials: true, search: urlSearchParams });

        return this._http.
            get(this._dynamicFormsUrl, requestOptions).
            map(this.dynamicFormFromData);
    }

    /**
     * Converts a Response into a DynamicForm
     */
    private dynamicFormFromData = (response: Response): DynamicForm => {
        let body = response.json();
        let dynamicControls: DynamicControl<any>[] = [];

        for (let dynamicControlData of body.dynamicControlDatas) {
            dynamicControls.push(new DynamicControl<any>(dynamicControlData, this));
        }

        return new DynamicForm(dynamicControls.sort((dynamicControl1, dynamicControl2) => dynamicControl1.order - dynamicControl2.order), body.errorMessage);
    }

    submitDynamicForm(url: string, dynamicForm: DynamicForm): Observable<Response> {
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let requestOptions = new RequestOptions({ withCredentials: true, headers: headers });

        return this.
            _http.
            post(url, JSON.stringify(dynamicForm.value), requestOptions);
    }

    public validateValue(url: string, value: string): Observable<Validity> {
        let urlSearchParams = new URLSearchParams();
        urlSearchParams.set(`value`, value);
        let requestOptions = new RequestOptions({ withCredentials: true, search: urlSearchParams });

        return this._http.
            get(url, requestOptions).
            map(this.validityFromData);
    }

    private validityFromData(response: Response): Validity{
        let body = response.json();

        return body.valid ? Validity.valid : Validity.invalid;
    }
}