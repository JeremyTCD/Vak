import { Injectable } from '@angular/core';
import { Response, URLSearchParams, Headers, RequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

import { environment } from '../../../environments/environment';
import { HttpService } from '../http.service';
import { ErrorHandlerService } from '../utility/error-handler.service';
import { DynamicForm } from './dynamic-form/dynamic-form';
import { DynamicFormData } from './dynamic-form/dynamic-form-data';
import { DynamicControl } from './dynamic-control/dynamic-control';
import { Validity } from './validity';

/**
 * Wraps Http requests and response processing for the dynamic forms system.
 */
@Injectable()
export class DynamicFormsService {
    private _dynamicFormsRelativeUrl = `DynamicForms/GetDynamicForm`;

    constructor(private _httpService: HttpService, private _errorHandlerService: ErrorHandlerService) { }

    /**
     * Sends get request to retrieve data for a DynamicForm.
     * Registers a function that maps retrieved data to a DynamicForm.
     * Registers catch function that calls ErrorHandlerService.handleUnexpectedError if
     * an error occurs.
     *
     * Returns
     * - Observable<DynamicForm> if get request succeeds.
     */
    getDynamicForm(formModelName: string): Observable<DynamicForm> {
        let urlSearchParams = new URLSearchParams();
        urlSearchParams.set(`formModelName`, formModelName);
        let requestOptions = new RequestOptions({withCredentials: true, search: urlSearchParams });

        return this._httpService.
            get(this._dynamicFormsRelativeUrl, requestOptions).
            map(this.dynamicFormFromData, this).
            catch(error => {
                this._errorHandlerService.handleUnexpectedError(error);
                return Observable.empty<DynamicForm>();
            });
    }

    /**
     * Converts a Response containing DynamicForm data into a DynamicForm
     *
     * Returns
     * - DynamicForm
     */
    private dynamicFormFromData(response: Response): DynamicForm {
        let dynamicFormData = response.json() as DynamicFormData;
        let dynamicControls: DynamicControl<any>[] = [];

        for (let dynamicControlData of dynamicFormData.dynamicControlDatas) {
            dynamicControls.push(new DynamicControl<any>(dynamicControlData, this));
        }

        let dynamicForm = new DynamicForm(dynamicControls.sort((dynamicControl1, dynamicControl2) => dynamicControl1.order - dynamicControl2.order), dynamicFormData.errorMessage);
        dynamicForm.setupContext();

        return dynamicForm;
    }

    /**
     * Sends post request to submit a DynamicForm's value.
     * Registers catch function handleSubmitDynamicFormError.
     *
     * Returns
     * - Observable<Response> if post request succeeds.
     * - Observable<{[key: string]: string}> containing validation errors (model state) if post request fails.
     *   Note that Http calls error observer if a response has status code >= 300 (Rx calls catch when error
     *   observer is called to allow error handling).
     */
    submitDynamicForm(relativeUrl: string, dynamicForm: DynamicForm): Observable<Response> {
        return this.
            _httpService.
            post(relativeUrl, dynamicForm.value).
            map(response => response.json()).
            catch(error => {
                return this.handleSubmitDynamicFormError(error);
            });
    }

    /**
     * Handles a submitDynamicForm error.
     *
     * Returns
     * - Error Observable with modelState value if error is a response with status 400 and json body with modelState
     *   as a key.
     * - Otherwise, returns an empty Observable.
     */
    private handleSubmitDynamicFormError(error: any): Observable<any> {
        if (error instanceof Response && error.status === 400) {
            let body = error.json();

            if (body && Object.keys(body).some(key => key === `modelState`)) {
                // Pass dictionary of errors to error function
                return Observable.throw(body.modelState);
            }
        }

        // Handle unexpected errors
        this._errorHandlerService.handleUnexpectedError(error);
        return Observable.empty<DynamicForm>();
    }

    /**
     * Sends get request to validate a value.
     * Registers a function that maps response to a boolean.
     *
     * Returns
     * - Observable<Validity> with value Validity.valid if value is valid and value Validity.invalid otherwise.
     * - Observable<Validity> with value Validity.valid if get request fails for user convenience.
     */
    validateValue(relativeUrl: string, value: string): Observable<Validity> {
        let urlSearchParams = new URLSearchParams();
        urlSearchParams.set(`value`, value);
        let requestOptions = new RequestOptions({ search: urlSearchParams });

        return this._httpService.
            get(relativeUrl, requestOptions).
            map(this.validityFromData).
            catch(error => {
                return Observable.of(Validity.valid);
            });
    }

    /**
     * Converts a Response containing a boolean into a Validity value.
     *
     * Returns
     * - Validity.invalid if body is defined and has a valid field with value false.
     * - Validity.valid otherwise. 
     */
    private validityFromData(response: Response): Validity{
        let body = response.json();

        if (body && body.valid === false) {
            return Validity.invalid;
        }

        return Validity.valid;
    }
}