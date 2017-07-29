import { Input, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { Document } from './document';
import { DocumentService } from './document.service';
import { JobService } from './job.service';

@Component({
    selector: 'createJob',
    templateUrl: './createJob.component.html',
    styleUrls: ['./createJob.component.css']
})
export class CreateJobComponent implements OnInit {
    documents: Document[] = [];
    allDocsSelected: boolean;
    constructor(private documentService: DocumentService, private jobService: JobService, private router: Router) {

    }

    ngOnInit(): void {
        this.documentService.getDocuments()
            .then(docs => this.documents = docs);
    }

    public selectAllDocuments(): void {

        this.allDocsSelected = !this.allDocsSelected;

        for (let doc of this.documents) {
            doc.isSelected = this.allDocsSelected;
        }
    }

    public startProcessingJob(): void {
        this.jobService.create()
            .then(job => this.router.navigate(['/jobs']))
    }
}