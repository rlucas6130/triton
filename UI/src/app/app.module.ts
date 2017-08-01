import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';

import { AppComponent } from './app.component';
import { JobsComponent } from './jobs.component';
import { CreateJobComponent } from './createJob.component';
import { JobService } from './job.service';
import { DocumentService } from './document.service';
import { AppRoutingModule } from './app-routing.module';

import { FileDropDirective, FileSelectDirective } from 'ng2-file-upload';

import { AlertModule, ModalModule, BsDropdownModule } from 'ngx-bootstrap';
import { FileUploader, FileUploaderOptions } from 'ng2-file-upload';

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
        CreateJobComponent,
        FileDropDirective,
        FileSelectDirective
    ],
    providers: [
        JobService,
        DocumentService,
        { provide: FileUploader, useFactory: () => { return new FileUploader({url: '', autoUpload: false} as FileUploaderOptions) } }
    ],
    bootstrap: [ AppComponent ]
})
export class AppModule { }
