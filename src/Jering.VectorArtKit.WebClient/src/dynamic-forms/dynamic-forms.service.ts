import { Injectable } from '@angular/core';
import { FormControl, FormGroup} from '@angular/forms';
import { Http, Response, RequestOptionsArgs, URLSearchParams } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { DynamicInput } from './dynamic-input-base';
import { DynamicInputValidators, DynamicInputValidator } from './dynamic-input-validators';

@Injectable()
export class DynamicFormsService {
    private _dynamicFormsUrl = 'http://localhost:62875/api/DynamicForms/GetDynamicForm';

    constructor(private _http: Http) {}

    /**
     * @description
     * Creates a FormGroup for the specified set of DynamicInputs.
     * Begins by creating a FormControl instance for each DynamicInput,
     * then wraps the FormControls in a FormGroup. FormGroups are used to
     * manage validation and state of a set of FormControls.
     * @param dynamicInputs
     */
    createFormGroup(dynamicInputs: DynamicInput<any>[]): FormGroup {
        let formControls: { [key: string]: FormControl } = {};

        for (let dynamicInput of dynamicInputs) {
            let validators: DynamicInputValidator[] = [];

            for (let validatorData of dynamicInput.validatorData) {
                validators.push(DynamicInputValidators[validatorData.name](validatorData, dynamicInput));
            }

            formControls[dynamicInput.name] = new FormControl(dynamicInput.initialValue || '', validators);
        }

        return new FormGroup(formControls);
    }

    getDynamicInputs(formModelName: string): Observable<DynamicInput<any>[]> {
        let urlSearchParams = new URLSearchParams();
        urlSearchParams.set("formModelName", formModelName);

        return this._http.
            get(this._dynamicFormsUrl, { search: urlSearchParams }).
            map(this.extractDynamicInputs);
    }

    extractDynamicInputs(response: Response): DynamicInput<any>[]  {
        let body = response.json()

        return (body.dynamicInputs as DynamicInput<any>[]).
            sort((dynamicInput1, dynamicInput2) => dynamicInput1.order - dynamicInput2.order);
    }
}
