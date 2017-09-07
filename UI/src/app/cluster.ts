
import { ClusterJobDocument } from "./ClusterJobDocument"; 
import { ClusterJobTerm } from "./ClusterJobTerm"; 

export interface Cluster 
{ 
    id: number;
    jobId: number;
    clusterCalculationId: number;
    centerVector: number[];
    si?: number;
    clusterJobDocuments: ClusterJobDocument[];
    clusterJobTerms: ClusterJobTerm[];
}