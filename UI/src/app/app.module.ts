import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';

import { AppComponent } from './app.component';
import { JobsComponent } from './jobs.component';
import { CreateJobComponent } from './createJob.component';
import { JobService } from './job.service';
import { AppRoutingModule } from './app-routing.module';

import { AlertModule, ModalModule, BsDropdownModule } from 'ngx-bootstrap';

@NgModule({
    imports: [
        BrowserModule,
        FormsModule,
        HttpModule,
        AppRoutingModule,
        AlertModule.forRoot(),
        ModalModule.forRoot(),
        BsDropdownModule.forRoot()
    ],
    declarations: [
        AppComponent,
        JobsComponent,
        CreateJobComponent
    ],
    providers: [ JobService ],
    bootstrap: [ AppComponent ]
})
export class AppModule { }
