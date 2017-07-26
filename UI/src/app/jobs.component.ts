import { Component, OnInit } from '@angular/core';

import { Job } from './job';
import { JobService } from './job.service';

@Component({
    selector: 'jobs',
    templateUrl: './jobs.component.html',
    styleUrls: ['./jobs.component.css']
})
export class JobsComponent implements OnInit {
    jobs: Job[] = [];
    jobStatus: {} = {
        New: JobStatus.New,
        BuildingMatrix: JobStatus.BuildingMatrix,
        SVD: JobStatus.SVD,
        Completed: JobStatus.Completed,
        Failed: JobStatus.Failed
    };

    constructor(private jobService: JobService) { }

    ngOnInit(): void {
        this.jobService.getJobs()
            .then(jobs => this.jobs = jobs);
    }
}

enum JobStatus {
    New, 
    BuildingMatrix,
    SVD,
    Completed,
    Failed
}