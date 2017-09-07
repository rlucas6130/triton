

export interface ClusterJobTerm 
{ 
    id: number;
    jobId: number;
    clusterCalculationId: number;
    jobTermId: number;
    clusterId: number;
    distanceToClusterCenter: number;
    vector: number[];
    value: string;
}