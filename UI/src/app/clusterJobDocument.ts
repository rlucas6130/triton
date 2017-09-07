

export interface ClusterJobDocument 
{ 
    id: number;
    jobId: number;
    clusterCalculationId: number;
    jobDocumentId: number;
    clusterId: number;
    si: number;
    vector: number[];
    name: string;
}