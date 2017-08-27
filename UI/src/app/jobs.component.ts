import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Job } from './job';
import { JobService } from './job.service';

import 'rxjs/add/operator/switchMap';

import * as THREE from 'three'; 

@Component({
    selector: 'jobs',
    templateUrl: './jobs.component.html',
    styleUrls: ['./jobs.component.css']
})
export class JobsComponent implements OnInit {
    jobs: Job[] = [];
    public jobStatus: {} = {
        New: JobStatus.New,
        BuildingMatrix: JobStatus.BuildingMatrix,
        SVD: JobStatus.SVD,
        Completed: JobStatus.Completed,
        Failed: JobStatus.Failed
    };

    constructor(private jobService: JobService, private route: ActivatedRoute) { }

    ngOnInit(): void {

        this.jobService.getJobs()
            .then(jobs => this.jobs = jobs);

        if (this.route.snapshot.paramMap.get('refreshUntilComplete') == 'true') {
            var jobsListIntervalId = setInterval(() => {

                this.jobService.getJobs()
                    .then(jobs => this.jobs = jobs)
                    .then(jobs => {

                        var processingJobs = jobs.filter((job) => job.status == JobStatus.New || job.status == JobStatus.SVD || job.status == JobStatus.BuildingMatrix);

                        if (processingJobs.length > 0) {
                            clearInterval(jobsListIntervalId); 
                        }

                        for (let job of processingJobs) {

                            var intervalId = setInterval((j: Job) => {

                                this.jobService.getJob(j.id).then(jj => {

                                    if (jj.status == JobStatus.Completed || jj.status == JobStatus.Failed) {                                      
                                        j.dimensions = jj.dimensions;
                                        clearInterval(intervalId);
                                    }

                                    j.totalCalculationTimeString = jj.totalCalculationTimeString;
                                    j.status = jj.status;
                                });
                            }, 1000, job);
                        }
                    });
            }, 5000);
        }
    }
}

enum JobStatus {
    New, 
    BuildingMatrix,
    SVD,
    Completed,
    Failed
}