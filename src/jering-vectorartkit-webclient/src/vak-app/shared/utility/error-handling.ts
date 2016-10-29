import { Response } from '@angular/http';
import { Router } from '@angular/router';
import { Check } from './check';
import { environment } from '../../../environments/environment';

export function handleUnexpectedError(router: Router, error: any) {
    if (Check.isString(error)) {
        router.navigate([`/error`, { errorMessage: error }]);
        return;
    }
    else if (error instanceof Response) {
        router.navigate([`/error`, { errorMessage: error.json().errorMessage }]);
        return;
    }
    else if (error instanceof Error && !environment.production) {
        router.navigate([`/error`,
            {
                errorMessage: `name: ${error.name}\n
                               message: ${error.message}\n
                               stack: ${error.stack}\n`
            }
        ]);
        return;
    }

    router.navigate([`/error`]);
}