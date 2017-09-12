import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { JobsComponent } from './jobs.component';
import { CreateJobComponent } from './createJob.component';
import { VisualizeJobComponent } from './visualizeJob.component';
import { ClusterCalculationComponent } from './clusterCalculation.component';

import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { LoadingIndicatorInterceptor } from './loadingIndicator.interceptor';
import { LoadingIndicatorService } from './loadingIndicator.service';

const routes: Routes = [
    {
        path: '',
        redirectTo: '/jobs',
        pathMatch: 'full'
    },
    {
        path: 'jobs',
        component: JobsComponent
    },
    {
        path: 'createJob',
        component: CreateJobComponent
    },
    {
        path: 'visualize/:jobId',
        component: VisualizeJobComponent
    },
    {
        path: 'clusterCalculation/:clusterCalculationId',
        component: ClusterCalculationComponent
    }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule { }