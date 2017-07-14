import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { JobsComponent } from './jobs.component';

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
    //{
    //    path: 'detail/:id',
    //    component: HeroDetailComponent
    //}
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule { }