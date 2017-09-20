import { Input, Component, OnInit, OnDestroy, DoCheck, AfterContentChecked, AfterViewChecked } from '@angular/core';
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
import { LoadingIndicatorService } from './loadingIndicator.service';
import { FileUploader, FileItem, FileUploaderOptions } from 'ng2-file-upload';
import { FileUploaderExtension } from './fileUploader.extension';
import * as Rx from 'rxjs/Rx';

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
export class CreateJobComponent implements OnInit, DoCheck, OnDestroy, AfterViewChecked { 
    documents: ViewDocument[] = [];
    masterDocuments: ViewDocument[] = [];
    allDocsSelected: boolean;
    initialUploadCount: number;
    docsLoaded: boolean = false;
    pageSize: number = 30;
    currentPage: number = 0;
    totalPages: number = 0;

    constructor(
        private documentService: DocumentService,
        private jobService: JobService,
        private router: Router,
        private uploader: FileUploaderExtension,
        private loadingIndicatorService: LoadingIndicatorService) {

    }

    ngAfterViewChecked(): void
    {
        this.loadingIndicatorService.toggle(false);
    }

    ngOnInit(): void {
        this.documentService.getDocuments()
            .subscribe(docs => {
                this.masterDocuments = docs as ViewDocument[];

                var start = this.pageSize * this.currentPage;

                this.documents = _.slice(this.masterDocuments, start, start + this.pageSize);

                this.totalPages = _.ceil(this.masterDocuments.length / this.pageSize);

                this.docsLoaded = true;
            });
    }

    ngOnDestroy(): void {
        if (this.timer) {
            clearInterval(this.timer);
        }
    }

    setFocusEvent(): void {
        var focus = Rx.Observable.fromEvent(window, 'focus').subscribe(event => {
            this.loadingIndicatorService.toggle(true);
            focus.unsubscribe();
        });
    }

    ngDoCheck(...args: any[]): void {
        if (this.uploader.queue.length > 0 && !this.uploader.isUploading) {

            this.uploader.queue = _.differenceWith(this.uploader.queue, this.masterDocuments,
                (file: FileItem, doc: Document) => doc.name == file.file.name && doc.id > 0);            

            if (this.uploader.queue.length > 0) {

                this.uploader.queue = _.uniqBy(this.uploader.queue, 'file.name').filter((fi) => {
                    return !_.some(_.filter(this.masterDocuments, (d) => d.id > 0), { name: fi.file.name });
                });

                let pageChange: boolean = false;

                for (let file of this.uploader.queue) {
                    if (!_.some(this.masterDocuments, { name: file.file.name })) {
                        this.masterDocuments.unshift({
                            id: 0,
                            name: file.file.name,
                            isSelected: false,
                            isUploading: false,
                            fileItem: file
                        } as ViewDocument);
                        pageChange = true;
                    }
                }

                if (pageChange) {
                    this.totalPages = _.ceil(this.masterDocuments.length / this.pageSize);
                    this.changePage(0);
                }
            }
        }
    }

    public getSelectedDocuments(): Document[]
    {
        return _.filter(this.masterDocuments, doc => doc.isSelected);
    }

    public getPercentCompleted(): number 
    {
        return Math.round((this.initialUploadCount - this.getUploadingDocuments().length) * 100 / this.initialUploadCount);
    }

    public getUploadingDocuments(): Document[]
    {
        return _.filter(this.masterDocuments, doc => doc.isUploading);
    }

    public getAvailableDocuments(): Document[]
    {
        return _.filter(this.masterDocuments, doc => doc.id > 0);
    }

    public selectAllDocuments(): void {

        this.allDocsSelected = !this.allDocsSelected;

        for (let doc of this.masterDocuments) {
            doc.isSelected = this.allDocsSelected;
        }
    }

    public uploadAll(): void {

        this.initialUploadCount = this.uploader.queue.length;

        this.uploader.uploadAllFiles();

        for (let doc of this.masterDocuments) {
            if (doc.id == 0) {
                doc.isUploading = true;
            }
        }

        this.waitUntilComplete();
    }

    public changePage(direction: number): void {
        this.currentPage += direction;

        var start = this.pageSize * this.currentPage;

        this.documents = _.slice(this.masterDocuments, start, start + this.pageSize);
    }

    public uploadDocument(document: ViewDocument): void {
        if (!document.isUploading) {

            this.initialUploadCount = 1;

            _.find(this.uploader.queue, (fI) => fI.file.name == document.name).upload();

            document.isUploading = true;

            this.waitUntilComplete();
        }
    }

    private timer: NodeJS.Timer;

    private waitUntilComplete(): void {
        this.timer = setInterval(() => {

            var uploadingDocs = _.filter(this.masterDocuments, (doc) => {
                return doc.id == 0 && doc.isUploading == true;
            });

            if (uploadingDocs.length == 0) {
                clearInterval(this.timer);
            }

            var sub = this.documentService.getDocuments(true)
                .subscribe(docs => {
                    for (let uploadingDoc of uploadingDocs) {
                        var savedDoc = _.find(docs, (doc) => doc.name == uploadingDoc.name);

                        if (savedDoc) {
                            var docsIndex = _.findIndex(this.masterDocuments, (doc) => doc.name == uploadingDoc.name);
                            this.masterDocuments[docsIndex].id = savedDoc.id;
                            this.masterDocuments[docsIndex].isUploading = false;
                            this.masterDocuments[docsIndex].isSelected = true;
                            this.masterDocuments[docsIndex].isNew = true;
                            this.masterDocuments[docsIndex].totalTermDocCount = savedDoc.totalTermDocCount;
                        }
                    }

                    sub.unsubscribe();
                });
        }, 5000);
    }

    public removeDocument(document: Document): void {
        _.find(this.uploader.queue, (fI) => fI.file.name == document.name).remove();

        _.remove(this.masterDocuments, document);
    }

    public removeAllUploads(): void {
        this.uploader.clearQueue();

        _.remove(this.masterDocuments, (doc) => doc.id == 0);
    }

    public docIdTracker(index: number, doc: Document): string {
        return doc.name;
    }

    public hideGlobalMenu(): boolean {
        return !_.some(this.masterDocuments, doc => doc.id == 0 && doc.isUploading == false);
    }

    public startProcessingJob(): void {

        var docIds = this.getSelectedDocuments().map(doc => doc.id);

        this.jobService.create(docIds)
            .then(job => this.router.navigate(['/jobs', { 'refreshUntilComplete': 'true' }]));
    }
}