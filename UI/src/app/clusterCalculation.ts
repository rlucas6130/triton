
import { Calculable } from "./calculable";
import { ClusterCalculationStatus } from "./ClusterCalculationStatus"; 
import { Cluster } from "./Cluster"; 

export interface ClusterCalculation extends Calculable
{ 
    id: number;
    clusterCount?: number;
    globalSi?: number;
    clusterSi?: number;
    jobId: number;
    minimumClusterCount: number;
    maximumClusterCount: number;
    iterationsPerCluster: number;
    maximumOptimizationsCount: number;
    status: ClusterCalculationStatus;
    clusters: Cluster[];
}