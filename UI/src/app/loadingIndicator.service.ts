import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';

@Injectable()
export class LoadingIndicatorService {
   
    constructor() { }

    private isLoadingSource = new Subject<boolean>();
    public isLoading$ = this.isLoadingSource.asObservable();

    toggle(show: boolean): void {
        this.isLoadingSource.next(show);
    }

    public hideLoadingIndicatorHook: string = 'hideloader';

    hide(url: string): string {
        return `${url}${(url.indexOf('?') > -1 ? '&' : '/?')}hideloader`;
    }
}