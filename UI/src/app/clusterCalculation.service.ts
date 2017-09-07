import { Injectable } from '@angular/core';
import { Headers, Http } from '@angular/http';

import 'rxjs/add/operator/toPromise';
import { Subject } from 'rxjs/Subject';

import { ClusterCalculation } from './clusterCalculation';
import { ClusterCalculationParameters } from './clusterCalculationParameters';

@Injectable()
export class ClusterCalculationService {
    private clusterCalculationUrl = '/api/clusterCalculations';
    private headers = new Headers({ 'Content-Type': 'application/json' });
    constructor(private http: Http) { }
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
    getClusterCalculations(jobId: number): Promise<ClusterCalculation[]> {
        const url = `${this.clusterCalculationUrl}/getAll?jobId=${jobId}`;
        return this.http.get(url)
            .toPromise()
            .then(response => response.json() as ClusterCalculation[])
            .catch(this.handleError);
    }
    getClusterCalculation(id: number): Promise<ClusterCalculation> {
        const url = `${this.clusterCalculationUrl}/${id}`;
        return this.http.get(url)
            .toPromise()
            .then(response => response.json() as ClusterCalculation)
            .catch(this.handleError);
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