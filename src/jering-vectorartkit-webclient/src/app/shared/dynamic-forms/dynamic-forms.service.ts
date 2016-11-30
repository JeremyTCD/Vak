import { Injectable } from '@angular/core';
import { Response, URLSearchParams, Headers, RequestOptions } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

import { environment } from '../../../environments/environment';
import { HttpService } from '../http.service';
import { DynamicForm } from './dynamic-form/dynamic-form';
import { DynamicFormResponseModel } from './response-models/dynamic-form.response-model';
import { ErrorResponseModel } from '../response-models/error.response-model';
import { ValidateResponseModel } from '../response-models/validate.response-model';
import { DynamicControl } from './dynamic-control/dynamic-control';
import { Validity } from './validity';

/**
 * Wraps Http requests and response processing for the dynamic forms system.
 */
@Injectable()
export class DynamicFormsService {
    private _dynamicFormsRelativeUrl = `DynamicForms/GetDynamicForm`;

    constructor(private _httpService: HttpService) { }

    /**
     * Sends get request to retrieve data for a DynamicForm.
     * Registers a function that maps retrieved data to a DynamicForm.
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
            map(this.dynamicFormFromResponseModel, this);
    }

    /**
     * Converts a DynamicFormResponseModel into a DynamicForm
     *
     * Returns
     * - DynamicForm
     */
    private dynamicFormFromResponseModel(formResponseModel: DynamicFormResponseModel): DynamicForm {
        let dynamicControls: DynamicControl[] = [];

        for (let controlResponseModel of formResponseModel.dynamicControlResponseModels) {
            dynamicControls.push(new DynamicControl(controlResponseModel, this));
        }

        let dynamicForm = new DynamicForm(dynamicControls.sort((dynamicControl1, dynamicControl2) => dynamicControl1.order - dynamicControl2.order),
            formResponseModel.errorMessage,
            formResponseModel.buttonText);
        dynamicForm.setupContext();

        return dynamicForm;
    }

    /**
     * Sends post request to submit a DynamicForm's value.
     * Registers catch function handleSubmitDynamicFormError.
     *
     * Returns
     * - Observable<ResponseModel> if post request succeeds.
     * - Observable<ErrorResponseModel> containing validation errors (model state) if form validation fails.
     */
    submitDynamicForm(relativeUrl: string, dynamicForm: DynamicForm): Observable<any> {
        return this.
            _httpService.
            post(relativeUrl, dynamicForm.value);
    }

    /**
     * Sends get request to validate a value.
     * Registers a function that maps response to a boolean.
     *
     * Returns
     * - Observable<Validity> with Validity.valid if value is valid
     * - Otherwise, Observable<Validity> with Validity.invalid 
     */
    validateValue(relativeUrl: string, value: string): Observable<Validity> {
        let urlSearchParams = new URLSearchParams();
        urlSearchParams.set(`value`, value);
        let requestOptions = new RequestOptions({ search: urlSearchParams });

        return this._httpService.
            get(relativeUrl, requestOptions).
            map(this.validityFromResponseModel);
    }

    /**
     * Converts a ValidityResponseModel into a Validity value.
     *
     * Returns
     * - Validity.invalid if ValidateResponseModel.valid is false
     * - Validity.valid otherwise
     */
    private validityFromResponseModel(responseModel: ValidateResponseModel): Validity{
        if (responseModel.valid === false) {
            return Validity.invalid;
        }

        return Validity.valid;
    }
}