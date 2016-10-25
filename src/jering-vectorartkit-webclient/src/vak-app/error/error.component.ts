import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';

@Component({
    templateUrl: './error.component.html'
})
export class ErrorComponent implements OnInit {
    public errorMessage: string;

    constructor(private _activatedRoute: ActivatedRoute) {
    }

    ngOnInit(): void {
        this._activatedRoute.
            params.
            subscribe((params: Params) => {
                this.errorMessage = params['errorMessage'];
            });
    }
}
