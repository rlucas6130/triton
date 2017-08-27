import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';

import { Job } from './job';
import { JobService, ClusterAnalysisParameters } from './job.service';

import 'rxjs/add/operator/switchMap';

@Component({
    selector: 'visualizeJob',
    templateUrl: './visualizeJob.component.html',
    styleUrls: ['./visualizeJob.component.css']
})
export class VisualizeJobComponent implements OnInit {
    job: Job = new Job();
    clusterParams: ClusterAnalysisParameters = {
        minimumClusterCount: 2,
        maximumClusterCount: 10,
        iterationsPerCluster: 1,
        maximumOptimizationsCount: 200
    };

    constructor(private jobService: JobService, private route: ActivatedRoute) {}

    ngOnInit(): void {
        this.route.paramMap
            .switchMap((params: ParamMap) => this.jobService.getJob(+params.get('jobId')))
            .subscribe(job => { this.job = job; console.log(this.job); });

    }

    createNewClusterJob(): void {
        this.jobService.clusterAnalysis(this.job.id, this.clusterParams);
    }
}