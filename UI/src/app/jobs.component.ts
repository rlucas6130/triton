  import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Job } from './job';
import { JobService } from './job.service';
import { Subscription } from 'rxjs/Subscription';

import 'rxjs/add/operator/switchMap';

import * as THREE from 'three'; 
import * as Rx from 'rxjs/Rx';
import * as _ from 'lodash'; 

@Component({
    selector: 'jobs',
    templateUrl: './jobs.component.html',
    styleUrls: ['./jobs.component.css']
})
export class JobsComponent implements OnInit, OnDestroy {
    jobs: Job[] = [];
    public jobStatus: {} = {
        New: JobStatus.New,
        BuildingMatrix: JobStatus.BuildingMatrix,
        SVD: JobStatus.SVD,
        Completed: JobStatus.Completed,
        Failed: JobStatus.Failed
    };

    constructor(private jobService: JobService, private route: ActivatedRoute) { }

    private jobsListSubscription: Subscription;
    private jobSubscriptions: Subscription[] = [];

    ngOnDestroy(): void {
        if (this.jobsListSubscription) {
            this.jobsListSubscription.unsubscribe();
        }

        if (this.jobSubscriptions && this.jobSubscriptions.length) {
            _.each(this.jobSubscriptions, subscription => subscription.unsubscribe());
        }
    }

    private createJobIntervals(jobs: Job[]) : void 
    {
        this.jobs = jobs;

        var processingJobs = this.jobs.filter((job) =>
            job.status.valueOf() == JobStatus.New ||
            job.status.valueOf() == JobStatus.SVD ||
            job.status.valueOf() == JobStatus.BuildingMatrix);

        for (let job of processingJobs) {

            if (!this.jobsListSubscription.closed) {
                this.jobsListSubscription.unsubscribe();
            }

            let subscription = Rx.Observable.timer(0,5000).switchMap(() => this.jobService.getJob(job.id, true)).subscribe(jj => {

                if (jj.status.valueOf() == JobStatus.Completed || jj.status.valueOf() == JobStatus.Failed) {
                    job.dimensions = jj.dimensions;
                    subscription.unsubscribe();
                }

                job.totalCalculationTimeString = jj.totalCalculationTimeString;
                job.status = jj.status;
            });

            this.jobSubscriptions.push(subscription);
        }
    }

    ngOnInit(): void {
        this.jobsListSubscription = this.route.snapshot.paramMap.get('refreshUntilComplete') == 'true' ?
            Rx.Observable.timer(0, 5000)
                .switchMap(() => this.jobService.getJobs())
                .subscribe(this.createJobIntervals.bind(this)) :
            this.jobService.getJobs()
                .subscribe(this.createJobIntervals.bind(this));
    }
}

enum JobStatus {
    New, 
    BuildingMatrix,
    SVD,
    Completed,
    Failed
}