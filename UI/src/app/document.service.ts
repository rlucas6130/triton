﻿import { Injectable } from '@angular/core';
import { Headers, Http } from '@angular/http';

import 'rxjs/add/operator/toPromise';

import { Job } from './job';

@Injectable()
export class DocumentService {
    private jobUrl = '/api/documents';
    private headers = new Headers({ 'Content-Type': 'application/json' });
    constructor(private http: Http) { }
    update(job: Job): Promise<Job> {
        const url = `${this.jobUrl}/${job.id}`;
        return this.http
            .put(url, JSON.stringify(job), { headers: this.headers })
            .toPromise()
            .then(() => job)
            .catch(this.handleError);
    }
    create(name: string): Promise<Job> {
        return this.http
            .post(this.jobUrl, JSON.stringify({ name: name }), { headers: this.headers })
            .toPromise()
            .then(res => res.json().data as Job)
            .catch(this.handleError);
    }
    delete(id: number): Promise<void> {
        const url = `${this.jobUrl}/${id}`;
        return this.http.delete(url, { headers: this.headers })
            .toPromise()
            .then(() => null)
            .catch(this.handleError);
    }
    getJobs(): Promise<Job[]> {
        return this.http.get(this.jobUrl)
            .toPromise()
            .then(response => response.json() as Job[])
            .catch(this.handleError)
    }
    private handleError(error: any): Promise<any> {
        console.error('An error occurred', error);
        return Promise.reject(error.message || error);
    }
    getJob(id: number): Promise<Job> {
        const url = `${this.jobUrl}/${id}`;
        return this.http.get(url)
            .toPromise()
            .then(response => response.json().data as Job)
            .catch(this.handleError);
    }
}