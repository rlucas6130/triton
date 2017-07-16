﻿import { Component, OnInit } from '@angular/core';

import { Job } from './job';
import { JobService } from './job.service';

@Component({
    selector: 'jobs',
    templateUrl: './jobs.component.html',
    styleUrls: ['./jobs.component.css']
})
export class JobsComponent implements OnInit {
    jobs: Job[] = [];
    jobStatusMap: {} = {
        0: 'New',

        1: 'Building Term/Doc Matrix',
        2: 'Running SVD',
        3: 'Completed',
        4: 'Failed'
    };

    constructor(private jobService: JobService) { }

    ngOnInit(): void {



        this.jobService.getJobs()
            .then(jobs => this.jobs = jobs);
    }
}