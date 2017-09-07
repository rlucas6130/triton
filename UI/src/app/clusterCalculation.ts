
import { Calculable } from "./calculable";
import { ClusterStatus } from "./ClusterStatus"; 
import { Cluster } from "./Cluster"; 

export interface ClusterCalculation extends Calculable
{ 
    id: number;
    clusterCount: number;
    globalSi: number;
    clusterSi: number;
    jobId: number;
    minimumClusterCount: number;
    maximumClusterCount: number;
    iterationsPerCluster: number;
    maximumOptimizationsCount: number;
    status: ClusterStatus;
    clusters: Cluster[];
}