import { Injectable } from '@angular/core';
import { HttpHeaders, HttpClient, HttpResponse } from '@angular/common/http';

import { Observable } from 'rxjs/Observable';

import 'rxjs/add/operator/toPromise';
import { LoadingIndicatorService } from './loadingIndicator.service';

import { Job } from './job';

@Injectable()
export class JobService {
    private jobUrl = '/api/jobs';
    private headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    constructor(private http: HttpClient, private loadingIndicatorService: LoadingIndicatorService) { }
    update(job: Job): Promise<Job> {
        const url = `${this.jobUrl}/${job.id}`;
        return this.http
            .put(url, JSON.stringify(job), { headers: this.headers })
            .toPromise()
            .then(() => job)
            .catch(this.handleError);
    }
    create(docIds: number[]): Promise<boolean> {
        return this.http
            .post(this.jobUrl, JSON.stringify(docIds), { headers: this.headers })
            .toPromise()
            .then(res => true)
            .catch(this.handleError);
    }
    delete(id: number): Promise<void> {
        const url = `${this.jobUrl}/${id}`;
        return this.http.delete(url, { headers: this.headers })
            .toPromise()
            .then(() => null)
            .catch(this.handleError);
    }
    getJobs(): Observable<Job[]> {
        return this.http.get<Job[]>(this.jobUrl);
    }
    getJob(id: number): Observable<Job> {
        const url = `${this.jobUrl}/${id}`;
        return this.http.get<Job>(url);
    }
    private handleError(error: any): Promise<any> {
        console.error('An error occurred', error);
        return Promise.reject(error.message || error);
    }
}