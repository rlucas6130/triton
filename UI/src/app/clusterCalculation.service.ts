import { Injectable } from '@angular/core';
import { HttpHeaders, HttpClient, HttpResponse } from '@angular/common/http';

import { Observable } from 'rxjs/Observable';

import 'rxjs/add/operator/toPromise';
import { Subject } from 'rxjs/Subject';

import { LoadingIndicatorService } from './loadingIndicator.service';

import { ClusterCalculation } from './clusterCalculation';
import { ClusterCalculationParameters } from './clusterCalculationParameters';

@Injectable()
export class ClusterCalculationService {
    private clusterCalculationUrl = '/api/clusterCalculations';
    private headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    constructor(private http: HttpClient, private loadingIndicatorService: LoadingIndicatorService) { }
    update(clusterCalculation: ClusterCalculation): Promise<ClusterCalculation> {
        const url = `${this.clusterCalculationUrl}/${clusterCalculation.id}`;
        return this.http
            .put(url, JSON.stringify(clusterCalculation), { headers: this.headers }) 
            .toPromise()
            .then(() => clusterCalculation)
            .catch(this.handleError);
    }
    create(parameters: ClusterCalculationParameters): Promise<boolean> { 
        return this.http
            .post(this.clusterCalculationUrl, JSON.stringify(parameters), { headers: this.headers })
            .toPromise()
            .then(res => true)
            .catch(this.handleError);
    }
    delete(id: number): Promise<void> {
        const url = `${this.clusterCalculationUrl}/${id}`;
        return this.http.delete(url, { headers: this.headers })
            .toPromise()
            .then(() => null)
            .catch(this.handleError);
    }
    getClusterCalculations(jobId: number, hideLoadingIndicator: boolean = false): Observable<ClusterCalculation[]> {
        const url = `${this.clusterCalculationUrl}/getAll?jobId=${jobId}`;

        return this.http.get<ClusterCalculation[]>(hideLoadingIndicator ?
            this.loadingIndicatorService.hide(url) : url);
            
    }
    getClusterCalculation(id: number, hideLoadingIndicator: boolean = false): Observable<ClusterCalculation> {
        const url = `${this.clusterCalculationUrl}/${id}`;
        return this.http.get<ClusterCalculation>(hideLoadingIndicator ?
            this.loadingIndicatorService.hide(url) : url);
    }

    private createClusterCalculationSource = new Subject<ClusterCalculationParameters>();
    public createClusterCalculation$ = this.createClusterCalculationSource.asObservable();

    createClusterCalculation(clusterCalculationParams: ClusterCalculationParameters): void {
        this.createClusterCalculationSource.next(clusterCalculationParams);
    }

    private handleError(error: any): Promise<any> {
        console.error('An error occurred', error);
        return Promise.reject(error.message || error);
    }
}