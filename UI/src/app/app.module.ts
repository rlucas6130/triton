import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';


import { AppComponent } from './app.component';
import { JobsComponent } from './jobs.component';
import { CreateJobComponent } from './createJob.component';
import { VisualizeJobComponent } from './visualizeJob.component';
import { ClusterCalculationModal } from './clusterCalculationModal.component';
import { JobService } from './job.service';
import { DocumentService } from './document.service';
import { ClusterCalculationService } from './clusterCalculation.service';
import { AppRoutingModule } from './app-routing.module';

import { FileDropDirective, FileSelectDirective } from 'ng2-file-upload';

import { ModalModule, PopoverModule  } from 'ngx-bootstrap';
import { FileUploader, FileUploaderOptions } from 'ng2-file-upload';

@NgModule({
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        FormsModule,
        HttpModule,
        AppRoutingModule,
        ModalModule.forRoot(),
        PopoverModule.forRoot()
    ],
    declarations: [
        AppComponent,
        JobsComponent,
        CreateJobComponent,
        VisualizeJobComponent,
        ClusterCalculationModal,
        FileDropDirective,
        FileSelectDirective
    ],
    providers: [
        JobService,
        DocumentService,
        ClusterCalculationService,
        {
            provide: FileUploader, useFactory: () => {
                return new FileUploader({
                    url: '/api/documents/upload',
                    removeAfterUpload: true
                } as FileUploaderOptions)
            }
        }
    ],
    entryComponents: [
        ClusterCalculationModal
    ],
    bootstrap: [ AppComponent ]
})
export class AppModule { }
