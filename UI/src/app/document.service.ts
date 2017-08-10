import { Injectable } from '@angular/core';
import { Headers, Http } from '@angular/http';

import 'rxjs/add/operator/toPromise';

import { Document } from './document';

@Injectable()
export class DocumentService {
    private documentUrl = '/api/documents';
    private headers = new Headers({ 'Content-Type': 'application/json' });
    constructor(private http: Http) { }
    update(document: Document): Promise<Document> {
        const url = `${this.documentUrl}/${document.id}`;
        return this.http
            .put(url, JSON.stringify(document), { headers: this.headers })
            .toPromise()
            .then(() => document)
            .catch(this.handleError);
    }
    create(name: string): Promise<Document> {
        return this.http
            .post(this.documentUrl, JSON.stringify({ name: name }), { headers: this.headers })
            .toPromise()
            .then(res => res.json().data as Document)
            .catch(this.handleError);
    }
    delete(id: number): Promise<void> {
        const url = `${this.documentUrl}/${id}`;
        return this.http.delete(url, { headers: this.headers })
            .toPromise()
            .then(() => null)
            .catch(this.handleError);
    }
    getDocuments(page: number = 1, docsPerPage: number = 20): Promise<Document[]> {
        return this.http.get(this.documentUrl, { params: { page: page, docsPerPage: docsPerPage } })
            .toPromise()
            .then(response => response.json() as Document[])
            .catch(this.handleError)
    }
    private handleError(error: any): Promise<any> {
        console.error('An error occurred', error);
        return Promise.reject(error.message || error);
    }
    getDocument(id: number): Promise<Document> {
        const url = `${this.documentUrl}/${id}`;
        return this.http.get(url)
            .toPromise()
            .then(response => response.json().data as Document)
            .catch(this.handleError);
    }
}