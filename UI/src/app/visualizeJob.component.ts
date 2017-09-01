import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';

import { Job } from './job';
import { JobService } from './job.service';

import { ClusterCalculation } from './clusterCalculation';
import { ClusterCalculationService, ClusterCalculationParameters } from './clusterCalculation.service';

import { BsModalService, BsModalRef } from 'ngx-bootstrap';

import 'rxjs/add/operator/switchMap';
import * as _ from 'lodash'; 

@Component({
    selector: 'visualizeJob',
    templateUrl: './visualizeJob.component.html',
    styleUrls: ['./visualizeJob.component.css']
})
export class VisualizeJobComponent implements OnInit {
    job: Job = new Job();
    clusterCalculations: ClusterCalculation[];
    clusterCalculationModal: BsModalRef;

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
        private route: ActivatedRoute,
        private modalService: BsModalService) { }

    ngOnInit(): void {
        this.route.paramMap
            .switchMap((params: ParamMap) => this.jobService.getJob(+params.get('jobId')))
            .subscribe(job => {
                this.job = job;
                this.clusterCalculationParams.jobId = this.job.id;
            });

        this.route.paramMap
            .switchMap((params: ParamMap) => this.clusterCalculationService.getClusterCalculations(+params.get('jobId')))
            .subscribe(clusterCalculations => {
                this.clusterCalculations = clusterCalculations;

                var processingClusterCalculations = clusterCalculations.filter((cc) => this.isClusterCalculating(cc));

                if (processingClusterCalculations.length > 0) {
                    this.refreshUntilCompleted(processingClusterCalculations);
                }
            });
    }

    private refreshUntilCompleted(processingClusterCalculations: ClusterCalculation[]): void {
        for (let clusterCalculations of processingClusterCalculations) {

            var intervalId = setInterval((cc: ClusterCalculation) => {

                this.clusterCalculationService.getClusterCalculation(cc.id).then(c => {

                    _.assign(cc, c);

                    if (c.status == ClusterCalculationStatus.Completed || c.status == ClusterCalculationStatus.Failed) {
                        clearInterval(intervalId);
                    }
                });
            }, 5000, clusterCalculations);
        }
    }

    createNewClusterJob(): void {
        this.clusterCalculationService.create(this.clusterCalculationParams).then(success => {
            if (success) {

                //Fire Initial
                this.clusterCalculationService.getClusterCalculations(this.job.id)
                    .then(ccs => this.clusterCalculations = ccs);

                var clusterCalculationListIntervalId = setInterval(() => {

                    this.clusterCalculationService.getClusterCalculations(this.job.id)
                        .then(ccs => this.clusterCalculations = ccs)
                        .then(ccs => {

                            var processingClusterCalculations = ccs.filter((cc) => this.isClusterCalculating(cc));

                            if (processingClusterCalculations.length > 0) {
                                clearInterval(clusterCalculationListIntervalId);

                                this.refreshUntilCompleted(processingClusterCalculations);
                            }
                        });
                }, 5000);
            } else {
                console.error("Cluster Calculation Creation Failed");
            }
        });
    }

    isClusterCalculating(clusterCalculation : ClusterCalculation) : boolean {
        return clusterCalculation.status == ClusterCalculationStatus.New || clusterCalculation.status == ClusterCalculationStatus.Clustering;
    }
}

enum ClusterCalculationStatus {
    New,
    Clustering,
    Completed,
    Failed
}