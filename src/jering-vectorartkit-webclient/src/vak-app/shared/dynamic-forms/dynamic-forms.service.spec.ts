import { TestBed, inject } from '@angular/core/testing';
import { Http, Response, ResponseOptions, RequestOptionsArgs } from '@angular/http';
import { Observable } from 'rxjs/Observable';

import { DynamicForm } from './dynamic-form/dynamic-form';
import { DynamicFormsService } from './dynamic-forms.service';
import { environment } from '../../../environments/environment';

let testFormModelName = `testFormModelName`;

describe('DynamicFormsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [DynamicFormsService, { provide: Http, useClass: HttpServiceStub }]
        });
    });

    it('getDynamicForm sends get request and processes response',
        inject([DynamicFormsService, Http], (dynamicFormsService: DynamicFormsService, httpServiceStub: HttpServiceStub) => {
            let spy = spyOn(httpServiceStub, `get`).and.callThrough();
            let result: DynamicForm;

            dynamicFormsService.
                getDynamicForm(testFormModelName).
                subscribe(dynamicForm => result = dynamicForm);

            let args = spy.calls.first().args;
            expect(args[0]).toBe(`${environment.apiUrl}DynamicForms/GetDynamicForm`);
            expect(args[1].search.get(`formModelName`)).toBe(testFormModelName);
            // Consider a more detailed test case
            expect(result instanceof DynamicForm).toBeTruthy();
        })
    );
});

let testResponse = new Response(
    new ResponseOptions({
            body: `{"dynamicControlDatas":{}}`
        })
);

class HttpServiceStub {
    get(url: string, options?: RequestOptionsArgs): Observable<Response> {
        return Observable.of(testResponse);
    };
}
