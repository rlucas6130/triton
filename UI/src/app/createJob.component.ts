import { Input, Component, OnInit, DoCheck } from '@angular/core';
import { Router, NavigationExtras } from '@angular/router';

import { Document } from './document';
import { DocumentService } from './document.service';
import { JobService } from './job.service';
import { FileUploader, FileItem } from 'ng2-file-upload';

import * as _ from 'lodash'; 

@Component({
    selector: 'createJob',
    templateUrl: './createJob.component.html',
    styleUrls: ['./createJob.component.css']
})
export class CreateJobComponent implements OnInit, DoCheck {
    documents: Document[] = [];
    allDocsSelected: boolean;
    constructor(
        private documentService: DocumentService,
        private jobService: JobService,
        private router: Router,
        private uploader: FileUploader) {

    }

    ngOnInit(): void {
        this.documentService.getDocuments()
            .then(docs => this.documents = docs);
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
                        isUploading: false
                    } as Document);
                }
            }
        }
    }

    public selectAllDocuments(): void {

        this.allDocsSelected = !this.allDocsSelected;

        for (let doc of this.documents) {
            doc.isSelected = this.allDocsSelected;
        }
    }

    public uploadAll(): void {

        this.uploader.uploadAll();

        for (let doc of this.documents) {
            if (doc.id == 0) {
                doc.isUploading = true;
            }
        }

        this.waitUntilComplete();
    }

    public uploadDocument(document: Document): void {
        _.find(this.uploader.queue, (fI) => fI.file.name == document.name).upload();

        document.isUploading = true;

        this.waitUntilComplete();
    }

    private waitUntilComplete(): void {
        var intervalId = setInterval(() => {

            var uploadingDocs = _.filter(this.documents, (doc) => {
                return doc.id == 0 && doc.isUploading == true;
            });

            if (uploadingDocs.length == 0) {
                clearInterval(intervalId);
            }

            this.documentService.getDocuments()
                .then(docs => {
                    for (let uploadingDoc of uploadingDocs) {
                        var savedDoc = _.find(docs, (doc) => doc.name == uploadingDoc.name);

                        if (savedDoc) {
                            var docsIndex = _.findIndex(this.documents, (doc) => doc.name == uploadingDoc.name);
                            this.documents[docsIndex].id = savedDoc.id;
                            this.documents[docsIndex].isUploading = false;
                        }
                    }
                });
        }, 1000);
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

    public startProcessingJob(): void {
        this.jobService.create()
            .then(job => this.router.navigate(['/jobs', { 'refreshUntilComplete': 'true' }]));
    }
}