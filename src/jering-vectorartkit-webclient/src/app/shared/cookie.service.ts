import { Injectable, Optional } from '@angular/core';

import { Check } from 'app/shared/check';

@Injectable()
export class CookieService {
    private _cookies: { [key: string]: string } = {};
    private _cookiesString: string = "";

    private getCookiesString(): string {
        return document.cookie || '';
    }

    cookieExists(name: string): boolean {
        return Check.isValue(this.cookies[name]);
    }

    private get cookies(): { [key: string]: string } {
        let cookiesString = this.getCookiesString();

        if (cookiesString !== this._cookiesString) {
            this._cookiesString = cookiesString;

            let cookieStrings: Array<string> = this._cookiesString.split('; ');
            this._cookies = {};
            for (let cookieString of cookieStrings) {
                let index: number = cookieString.indexOf('=');

                if (index > 0) {  // ignore nameless cookies
                    let name: string = this.safeDecodeURIComponent(cookieString.substring(0, index));
                    this._cookies[name] = this.safeDecodeURIComponent(cookieString.substring(index + 1));
                }
            }
        }

        return this._cookies;
    }

    private safeDecodeURIComponent(str: string) {
        try {
            return decodeURIComponent(str);
        } catch (e) {
            return str;
        }
    }
}