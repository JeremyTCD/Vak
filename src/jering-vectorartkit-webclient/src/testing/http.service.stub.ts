import { Observable } from 'rxjs/Observable';
import { RequestOptionsArgs } from '@angular/http';

export class StubHttpService {
    get(url: string, options?: RequestOptionsArgs, domain?: string): Observable<any> {
        return Observable.of(null);
    };

    post(url: string, body: string, options?: RequestOptionsArgs, domain?: string): Observable<any> {
        return Observable.of(null);
    };
}