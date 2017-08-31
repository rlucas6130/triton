import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';

import { Job } from './job';
import { JobService } from './job.service';

import { ClusterCalculation } from './clusterCalculation';
import { ClusterCalculationService, ClusterCalculationParameters } from './clusterCalculation.service';

import 'rxjs/add/operator/switchMap';

@Component({
    selector: 'visualizeJob',
    templateUrl: './visualizeJob.component.html',
    styleUrls: ['./visualizeJob.component.css']
})
export class VisualizeJobComponent implements OnInit {
    job: Job = new Job();
    clusterCalculations: ClusterCalculation[];

    clusterCalculationParams: ClusterCalculationParameters = {
        minimumClusterCount: 2,
        maximumClusterCount: 10,
        iterationsPerCluster: 1,
        maximumOptimizationsCount: 200
    };

    public clusterCalculationStatus: {} = {
        New: ClusterCalculationStatus.New,
        Clustering: ClusterCalculationStatus.Clustering,
        Completed: ClusterCalculationStatus.Completed,
        Failed: ClusterCalculationStatus.Failed
    };

    constructor(
        private jobService: JobService,
        private clusterCalculationService: ClusterCalculationService,
        private route: ActivatedRoute) { }

    ngOnInit(): void {
        this.route.paramMap
            .switchMap((params: ParamMap) => this.jobService.getJob(+params.get('jobId')))
            .subscribe(job => { this.job = job; });

        this.route.paramMap
            .switchMap((params: ParamMap) => this.clusterCalculationService.getClusterCalculations(+params.get('jobId')))
            .subscribe(clusterCalculations => { this.clusterCalculations = clusterCalculations; });
    }

    createNewClusterJob(): void {
        this.clusterCalculationService.create(this.job.id, this.clusterCalculationParams);
    }
}

enum ClusterCalculationStatus {
    New,
    Clustering,
    Completed,
    Failed
}