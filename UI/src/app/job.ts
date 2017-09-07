
import { Calculable } from "./calculable";
import { JobStatus } from "./JobStatus"; 

export interface Job extends Calculable
{ 
    id: number;
    documentCount: number;
    dimensions: number;
    status: JobStatus;
}