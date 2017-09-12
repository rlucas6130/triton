import { Input, Component, OnInit, DoCheck } from '@angular/core';
import { Router, NavigationExtras } from '@angular/router';
import {
    trigger,
    state,
    style,
    animate,
    transition
} from '@angular/animations';

import { Document } from './document';
import { DocumentService } from './document.service';
import { JobService } from './job.service';
import { FileUploader, FileItem } from 'ng2-file-upload';

import * as _ from 'lodash'; 

type ViewDocument = Document & {
    isSelected: boolean,
    isUploading: boolean,
    isNew?: boolean,
    FileItem?: FileItem
};

@Component({
    selector: 'createJob',
    templateUrl: './createJob.component.html',
    styleUrls: ['./createJob.component.css']
})
export class CreateJobComponent implements OnInit, DoCheck { 
    documents: ViewDocument[] = [];
    allDocsSelected: boolean;
    initialUploadCount: number;
    docsLoaded: boolean = false;
    constructor(
        private documentService: DocumentService,
        private jobService: JobService,
        private router: Router,
        private uploader: FileUploader) {

    }

    ngOnInit(): void {
        this.documentService.getDocuments()
            .subscribe(docs => {
                this.documents = docs as ViewDocument[];
                this.docsLoaded = true;
            });
    }

    ngDoCheck(...args: any[]): void {

        if (this.uploader.queue.length > 0 && !this.uploader.isUploading) {

            this.uploader.queue = _.uniqBy(this.uploader.queue, 'file.name')

            this.uploader.queue = _.filter(this.uploader.queue, (fi) => {
                return !_.some(_.filter(this.documents, (d) => d.id > 0), { name: fi.file.name });
            });

            for (let file of this.uploader.queue) {
                if (!_.some(this.documents, { name: file.file.name })) {
                    this.documents.unshift({
                        id: 0,
                        name: file.file.name,
                        isSelected: false,
                        isUploading: false,
                        fileItem: file
                    } as ViewDocument);
                }
            }
        }
    }

    public getSelectedDocuments(): Document[]
    {
        return _.filter(this.documents, doc => doc.isSelected);
    }

    public getPercentCompleted(): number 
    {
        return Math.round((this.initialUploadCount - this.getUploadingDocuments().length) * 100 / this.initialUploadCount);
    }

    public getUploadingDocuments(): Document[]
    {
        return _.filter(this.documents, doc => doc.isUploading);
    }

    public getAvailableDocuments(): Document[]
    {
        return _.filter(this.documents, doc => doc.id > 0);
    }

    public selectAllDocuments(): void {

        this.allDocsSelected = !this.allDocsSelected;

        for (let doc of this.documents) {
            doc.isSelected = this.allDocsSelected;
        }
    }

    public uploadAll(): void {

        this.initialUploadCount = this.uploader.queue.length;

        this.uploader.uploadAll();

        for (let doc of this.documents) {
            if (doc.id == 0) {
                doc.isUploading = true;
            }
        }

        this.waitUntilComplete();
    }

    public uploadDocument(document: ViewDocument): void {
        if (!document.isUploading) {

            this.initialUploadCount = 1;

            _.find(this.uploader.queue, (fI) => fI.file.name == document.name).upload();

            document.isUploading = true;

            this.waitUntilComplete();
        }
    }

    private waitUntilComplete(): void {
        var intervalId = setInterval(() => {

            var uploadingDocs = _.filter(this.documents, (doc) => {
                return doc.id == 0 && doc.isUploading == true;
            });

            if (uploadingDocs.length == 0) {
                clearInterval(intervalId);
            }

            this.documentService.getDocuments(true)
                .subscribe(docs => {
                    for (let uploadingDoc of uploadingDocs) {
                        var savedDoc = _.find(docs, (doc) => doc.name == uploadingDoc.name);

                        if (savedDoc) {
                            var docsIndex = _.findIndex(this.documents, (doc) => doc.name == uploadingDoc.name);
                            this.documents[docsIndex].id = savedDoc.id;
                            this.documents[docsIndex].isUploading = false;
                            this.documents[docsIndex].isSelected = true;
                            this.documents[docsIndex].isNew = true;
                            this.documents[docsIndex].totalTermDocCount = savedDoc.totalTermDocCount;
                        }
                    }
                });
        }, 5000);
    }

    public removeDocument(document: Document): void {
        _.find(this.uploader.queue, (fI) => fI.file.name == document.name).remove();

        _.remove(this.documents, document);
    }

    public removeAllUploads(): void {
        this.uploader.clearQueue();

        _.remove(this.documents, (doc) => doc.id == 0);
    }

    public docIdTracker(index: number, doc: Document): string {
        return doc.name;
    }

    public hideGlobalMenu(): boolean {
        return !_.some(this.documents, doc => doc.id == 0 && doc.isUploading == false);
    }

    public startProcessingJob(): void {

        var docIds = this.getSelectedDocuments().map(doc => doc.id);

        this.jobService.create(docIds)
            .then(job => this.router.navigate(['/jobs', { 'refreshUntilComplete': 'true' }]));
    }
}