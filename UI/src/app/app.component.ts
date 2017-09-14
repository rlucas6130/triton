import { Component, ApplicationRef } from '@angular/core';
import { LoadingIndicatorService } from './loadingIndicator.service';

@Component({
    selector: 'triton-app',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css'],
})
export class AppComponent  {
    title = 'Triton';

    private isLoading: boolean = true;

    constructor(private loadingIndicatorService: LoadingIndicatorService, private applicationRef: ApplicationRef) {
        loadingIndicatorService.isLoading$.subscribe(isLoading => {
            setTimeout(() => this.isLoading = isLoading);
        });
    }
} 
