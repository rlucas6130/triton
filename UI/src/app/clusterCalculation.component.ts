import { Component, OnInit, ViewChild, ElementRef  } from '@angular/core';
import { ClusterCalculation } from './clusterCalculation';
import { Cluster } from './cluster';
import { ClusterJobDocument } from './clusterJobDocument';
import { ClusterCalculationParameters } from './clusterCalculationParameters';
import { ClusterCalculationService } from './clusterCalculation.service';
import { ActivatedRoute, ParamMap } from '@angular/router';

import 'rxjs/add/operator/switchMap';
import * as _ from 'lodash'; 

type ViewCluster = Cluster & {
    x: number,
    y: number,
    size: number,
    popoverTitle: string,
    popoverContent: string,
    fill: string, 
    stroke: string
}

type ViewClusterJobDocument = ClusterJobDocument & {
    x: number,
    y: number,
    stroke: string, 
    fill: string
}

@Component({
    selector: 'cluster-calculation',
    templateUrl: './clusterCalculation.component.html',
    styleUrls: ['./clusterCalculation.component.css'],
})
export class ClusterCalculationComponent implements OnInit {

    clusterCalculation: ClusterCalculation;
    clusters: ViewCluster[];
    documents: ViewClusterJobDocument[];
    activeCluster: ViewCluster;
    svgHeight: number;
    svgWidth: number;
    maxClusterSizePct: number = 5;
    maxClusterSizePx: () => number =
        () => (this.svgWidth * this.maxClusterSizePct) / 100;

    constructor(
        private clusterCalculationService: ClusterCalculationService,
        private route: ActivatedRoute) {
    }

    ngOnInit(): void {
        this.route.paramMap
            .switchMap((params: ParamMap) => this.clusterCalculationService.getClusterCalculation(+params.get('clusterCalculationId')))
            .subscribe(clusterCalculation => {
                console.log(clusterCalculation);
                this.clusterCalculation = clusterCalculation;
                this.clusters = clusterCalculation.clusters as ViewCluster[];

                
                this.computeClusterSizes();
            });

        this.resize();
    }

    private computeClusterSizes(): void {
        if (this.clusters) {
            this.clusters = _.orderBy(this.clusters, c => c.clusterJobDocuments.length, 'desc');

            console.log(this.clusters);

            var largestClusterSize = _.maxBy(this.clusters, c => c.clusterJobDocuments.length)
                .clusterJobDocuments.length;

            let jobDocs: ClusterJobDocument[] = _.flatMap(this.clusters, c => c.clusterJobDocuments);

            var xValues = this.filterOutliers(_.map(this.clusters, c => c.centerVector[0]));
            var yValues = this.filterOutliers(_.map(this.clusters, c => c.centerVector[1]));

            var maxX = _.max(xValues);
            var maxY = _.max(yValues);

            var minX = _.min(xValues);
            var minY = _.min(yValues);

            console.log(maxX, maxY, minX, minY);

            var paddingPct = .1 // 10%

            var xRange = Math.abs(maxX - minX) + (Math.abs(maxX - minX) * paddingPct * 2);
            var yRange = Math.abs(maxY - minY) + (Math.abs(maxY - minY) * paddingPct * 2) ;

            console.log("Ranges", xRange, yRange);

            for (let cluster of this.clusters) {

                let sliceX = Math.abs(cluster.centerVector[0] - (minX - (Math.abs(maxX - minX) * paddingPct)));
                let sliceY = Math.abs(cluster.centerVector[1] - (minY - (Math.abs(maxY - minY) * paddingPct)));

                cluster.x = (sliceX * this.svgWidth) / xRange;
                cluster.y = (sliceY * this.svgHeight) / yRange;
                cluster.size = (this.maxClusterSizePx() * cluster.clusterJobDocuments.length) / largestClusterSize;

                let siAbs = Math.abs(cluster.si);

                cluster.fill = siAbs > .5 ? "green" : siAbs > .4 && siAbs <= .5 ? "blue" : siAbs > .25 && siAbs <= .4 ? "yellow" : siAbs <= .25 ? "red" : "red";
                cluster.stroke = siAbs > .5 ? "darkgreen" : siAbs > .4 && siAbs <= .5 ? "darkblue" : siAbs > .25 && siAbs <= .4 ? "orange" : siAbs <= .25 ? "darkred" : "darkred";

                cluster.clusterJobTerms = _.orderBy(cluster.clusterJobTerms, jt => jt.distanceToClusterCenter);
                cluster.clusterJobDocuments = _.orderBy(cluster.clusterJobDocuments, jd => jd.si);

                cluster.popoverTitle = `${cluster.clusterJobTerms[0].value}, ${cluster.clusterJobTerms[1].value}, ${cluster.clusterJobTerms[2].value} , ${cluster.clusterJobTerms[3].value}`;

                if (cluster.id == 2643) {
                    this.documents = cluster.clusterJobDocuments as ViewClusterJobDocument[];

                    for (let cjd of this.documents) {
                        let sliceXDoc = Math.abs(cjd.vector[0] - (minX - (Math.abs(maxX - minX) * paddingPct)));
                        let sliceYDoc = Math.abs(cjd.vector[1] - (minY - (Math.abs(maxY - minY) * paddingPct)));

                        cjd.x = (sliceXDoc * this.svgWidth) / xRange;
                        cjd.y = (sliceYDoc * this.svgHeight) / yRange;
                        cjd.stroke = "black";
                        cjd.fill = "black";
                    }
                }
            }
        }
    }

    resize(): void {
        this.svgHeight = window.innerHeight - 56 - 44 - 36;
        this.svgWidth = window.innerWidth - 320 - 60;

        this.computeClusterSizes();
    }

    private filterOutliers(coordinates: number[]) {  

        // Copy the values, rather than operating on references to existing values
        var values = coordinates.concat();

        // Then sort
        values.sort(function (a, b) {
            return a - b;
        });

        /* Then find a generous IQR. This is generous because if (values.length / 4) 
         * is not an int, then really you should average the two elements on either 
         * side to find q1.
         */
        var q1 = values[Math.floor((values.length / 4))];
        // Likewise for q3. 
        var q3 = values[Math.ceil((values.length * (3 / 4)))];
        var iqr = q3 - q1;

        // Then find min and max values
        var maxValue = q3 + iqr * 1.5;
        var minValue = q1 - iqr * 1.5;

        // Then filter anything beyond or beneath these values.
        var filteredValues = values.filter(function (x) {
            return (x < maxValue) && (x > minValue);
        });

        // Then return
        return filteredValues;
    }
} 
