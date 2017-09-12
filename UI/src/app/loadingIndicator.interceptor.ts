import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { LoadingIndicatorService } from './loadingIndicator.service';

import 'rxjs/add/operator/do';

@Injectable()
export class LoadingIndicatorInterceptor implements HttpInterceptor {

    constructor(private loadingIndicatorService: LoadingIndicatorService) { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).do(event => {

            if (req.urlWithParams.indexOf('hideloader') > -1 || req.method === "POST") {
                return;
            }

            if (event instanceof HttpResponse) {
                this.loadingIndicatorService.toggle(false);
            } else {
                this.loadingIndicatorService.toggle(true);
            }
        });
    }
}