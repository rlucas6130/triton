import { Component, OnInit, ViewChild, ElementRef  } from '@angular/core';
import { ClusterCalculation } from './clusterCalculation';
import { ClusterCalculationService, ClusterCalculationParameters } from './clusterCalculation.service';
import { ActivatedRoute, ParamMap } from '@angular/router';

import 'rxjs/add/operator/switchMap';

@Component({
    selector: 'cluster-calculation',
    templateUrl: './clusterCalculation.component.html',
    styleUrls: ['./clusterCalculation.component.css'],
})
export class ClusterCalculationComponent implements OnInit {

    clusterCalculation: ClusterCalculation;
    svgHeight: number;
    svgWidth: number;

    constructor(
        private clusterCalculationService: ClusterCalculationService,
        private route: ActivatedRoute) {
    }

    ngOnInit(): void {
        this.route.paramMap
            .switchMap((params: ParamMap) => this.clusterCalculationService.getClusterCalculation(+params.get('clusterCalculationId')))
            .subscribe(clusterCalculation => {
                this.clusterCalculation = clusterCalculation;
            });

        this.resize();
    }

    resize(): void {
        this.svgHeight = window.innerHeight - 56 - 44 - 36;
        this.svgWidth = window.innerWidth - 320 - 60;
    }
} 
