import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';

@Injectable()
export class LoadingIndicatorService {
   
    constructor() { }

    private isLoadingSource = new Subject<boolean>();
    private is
    public isLoading$ = this.isLoadingSource.asObservable();

    toggle(show: boolean): void {
        this.isLoadingSource.next(show);
    }

    getIsLoading(): bool {
        return isLoading;
    }
}