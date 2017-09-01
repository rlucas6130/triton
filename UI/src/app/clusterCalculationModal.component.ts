import { Component, OnInit  } from '@angular/core';
import { ClusterCalculation } from './clusterCalculation';
import { ClusterCalculationService, ClusterCalculationParameters } from './clusterCalculation.service';
import { BsModalService, BsModalRef } from 'ngx-bootstrap';

@Component({
    selector: 'cluster-calculation-modal',
    templateUrl: './clusterCalculationModal.component.html',
    styleUrls: ['./clusterCalculationModal.component.css'],
})
export class ClusterCalculationModal implements OnInit {

    clusterCalculationParams: ClusterCalculationParameters = {
        minimumClusterCount: 2,
        maximumClusterCount: 10,
        iterationsPerCluster: 1,
        maximumOptimizationsCount: 200
    };

    ngOnInit(): void {

    }

    constructor(private modalRef: BsModalRef, private clusterCalculationService: ClusterCalculationService) {

    }

    createNewClusterJob(): void {
        this.clusterCalculationService.createClusterCalculation(this.clusterCalculationParams);
        this.modalRef.hide();
    }
} 
