import { Component, OnInit } from '@angular/core';

import { Job } from './job';
import { JobService } from './job.service';

@Component({
    selector: 'createJob',
    templateUrl: './createJob.component.html',
    styleUrls: ['./createJob.component.css']
})
export class CreateJobComponent implements OnInit {
    jobs: Job[] = [];

    constructor(private jobService: JobService) {

    }

    ngOnInit(): void {
        this.jobService.getJobs()
            .then(jobs => this.jobs = jobs);
    }
}