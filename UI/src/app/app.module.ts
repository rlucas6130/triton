import { NgModule, Optional, Inject }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { ModalModule, PopoverModule } from 'ngx-bootstrap';
import { FileUploader, FileUploaderOptions } from 'ng2-file-upload';
import { FileUploaderExtension } from './fileUploader.extension';
 
import { RouterModule } from '@angular/router';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { AppComponent } from './app.component';
import { JobsComponent } from './jobs.component';
import { CreateJobComponent } from './createJob.component';
import { VisualizeJobComponent } from './visualizeJob.component';
import { ClusterCalculationComponent } from './clusterCalculation.component';
import { ClusterCalculationModal } from './clusterCalculationModal.component';
import { LoadingIndicatorService } from './loadingIndicator.service';
import { JobService } from './job.service';
import { DocumentService } from './document.service';
import { ClusterCalculationService } from './clusterCalculation.service';
import { AppRoutingModule } from './app-routing.module';

import { FileDropDirective, FileSelectDirective } from 'ng2-file-upload';
import { LoadingIndicatorInterceptor } from './loadingIndicator.interceptor';

@NgModule({
    imports: [
        BrowserModule,    
        BrowserAnimationsModule,
        FormsModule,
        AppRoutingModule,
        ModalModule.forRoot(),
        PopoverModule.forRoot(),
        HttpClientModule
    ],
    declarations: [
        AppComponent,
        JobsComponent,
        CreateJobComponent,
        VisualizeJobComponent,
        ClusterCalculationModal,
        ClusterCalculationComponent,
        FileDropDirective,
        FileSelectDirective
    ],
    providers: [
        {
            provide: HTTP_INTERCEPTORS,
            useClass: LoadingIndicatorInterceptor,
            multi: true,
        },
        JobService,
        DocumentService,
        ClusterCalculationService,
        LoadingIndicatorService,
        {
            provide: FileUploaderExtension, useFactory: () => {
                return new FileUploaderExtension({
                    url: '/api/documents/upload',
                    removeAfterUpload: true,
                    allowedMimeType: ['text/html']
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
