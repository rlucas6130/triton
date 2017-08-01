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
    uploadQueue: FileItem[] = [];
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

        this.uploader.queue = _.uniqBy(this.uploader.queue, 'file.name');

        for (let file of this.uploader.queue)
        {
            this.documents.unshift({ 
                id: 0,
                name: file.file.name,
                isSelected: false        
            } as Document);
        }

        this.documents = _.uniqBy(this.documents, 'name');
    }

    public selectAllDocuments(): void {

        this.allDocsSelected = !this.allDocsSelected;

        for (let doc of this.documents) {
            doc.isSelected = this.allDocsSelected;
        }
    }

    public startProcessingJob(): void {
        this.jobService.create()
            .then(job => this.router.navigate(['/jobs', { 'refreshUntilComplete': 'true' }]));
    }
}