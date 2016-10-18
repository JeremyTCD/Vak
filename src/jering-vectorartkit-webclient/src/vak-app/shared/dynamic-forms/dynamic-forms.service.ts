import { Injectable } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Http, Response, RequestOptionsArgs, URLSearchParams } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { environment } from '../../../environments/environment';
import { DynamicForm } from './dynamic-form/dynamic-form';
import { DynamicInput } from './dynamic-input/dynamic-input';
import { DynamicInputValidators, DynamicInputValidator } from './dynamic-input/dynamic-input-validators';

@Injectable()
export class DynamicFormsService {
    private _dynamicFormsUrl = `${environment.apiUrl}DynamicForms/GetDynamicForm`;

    constructor(private _http: Http) { }

    /**
     * @description
     * Creates a FormGroup for the specified set of DynamicInputs.
     * Begins by creating a FormControl instance for each DynamicInput,
     * then wraps the FormControls in a FormGroup. FormGroups are used to
     * manage validation and state of a set of FormControls.
     * @param dynamicInputs
     */
    createDynamicForm(dynamicInputs: DynamicInput<any>[]): DynamicForm {
        for (let dynamicInput of dynamicInputs) {
            let validators: DynamicInputValidator[] = [];

            for (let validatorData of dynamicInput.validatorData) {
                validators.push(DynamicInputValidators[validatorData.name](validatorData, dynamicInput));
            }

            dynamicInput.validators = validators;
        }

        return new DynamicForm(dynamicInputs);
    }

    getDynamicInputs(formModelName: string): Observable<DynamicInput<any>[]> {
        let urlSearchParams = new URLSearchParams();
        urlSearchParams.set("formModelName", formModelName);

        return this._http.
            get(this._dynamicFormsUrl, { search: urlSearchParams }).
            map(this.extractDynamicInputs);
    }

    extractDynamicInputs(response: Response): DynamicInput<any>[] {
        let body = response.json()
        let result: DynamicInput<any>[] = [];

        for (let obj of body.dynamicInputs) {
            result.push(new DynamicInput<any>(obj));
        }

        return result.sort((dynamicInput1, dynamicInput2) => dynamicInput1.order - dynamicInput2.order);
    }
}
