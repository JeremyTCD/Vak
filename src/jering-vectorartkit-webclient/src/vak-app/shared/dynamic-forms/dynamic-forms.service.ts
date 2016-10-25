import { Injectable } from '@angular/core';
import { Http, Response, URLSearchParams, Headers, RequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { ErrorObservable } from 'rxjs/observable/ErrorObservable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

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
        let requestOptions = new RequestOptions({withCredentials: true, search: urlSearchParams });

        return this._http.
            get(this._dynamicFormsUrl, requestOptions).
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

        return new DynamicForm(dynamicControls.sort((dynamicControl1, dynamicControl2) => dynamicControl1.order - dynamicControl2.order), body.errorMessage);
    }

    submitDynamicForm(url: string, dynamicForm: DynamicForm): Observable<Response> {
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let requestOptions = new RequestOptions({ withCredentials: true, headers: headers });

        return this.
            _http.
            post(url, JSON.stringify(dynamicForm.value), requestOptions).
            catch(this.handleThrownError);
    }

    /**
     * If an error is caught, return an ErrorObservable. Note that an ErrorObservable immediately emits an error notification. 
     */
    handleThrownError(error: any): ErrorObservable {
        return Observable.throw(error);
    }
}