import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { JobsComponent } from './jobs.component';
import { CreateJobComponent } from './createJob.component';
import { VisualizeJobComponent } from './visualizeJob.component';

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
    }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule { }