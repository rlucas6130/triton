import { Injectable } from '@angular/core';
import { HttpHeaders, HttpClient, HttpResponse, HttpParams } from '@angular/common/http';

import { Observable } from 'rxjs/Observable';
import { LoadingIndicatorService } from './loadingIndicator.service';

import 'rxjs/add/operator/toPromise';

import { Document } from './document';

@Injectable()
export class DocumentService {
    private documentUrl = '/api/documents';
    private headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    constructor(private http: HttpClient, private loadingIndicatorService: LoadingIndicatorService) { }
    update(document: Document): Promise<Document> {
        const url = `${this.documentUrl}/${document.id}`;
        return this.http
            .put(url, JSON.stringify(document), { headers: this.headers })
            .toPromise()
            .then(() => document)
            .catch(this.handleError);
    }
    create(name: string): Observable<Document> {
        return this.http
            .post<Document>(this.documentUrl, JSON.stringify({ name: name }), { headers: this.headers });
    }
    delete(id: number): Promise<void> {
        const url = `${this.documentUrl}/${id}`;
        return this.http.delete(url, { headers: this.headers })
            .toPromise()
            .then(() => null)
            .catch(this.handleError);
    }
    getDocuments(hideLoadingIndicator: boolean = false): Observable<Document[]> {
        return this.http.get<Document[]>(hideLoadingIndicator ?
            this.loadingIndicatorService.hide(this.documentUrl) : this.documentUrl);
    }
    private handleError(error: any): Promise<any> {
        console.error('An error occurred', error);
        return Promise.reject(error.message || error);
    }
    getDocument(id: number): Observable<Document> {
        const url = `${this.documentUrl}/${id}`;
        return this.http.get<Document>(url);
    }
}