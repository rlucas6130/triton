import { Component } from '@angular/core';

@Component({
  selector: 'my-app',
  template: `
        <h1>{{title}}</h1>
        <nav>
            <a routerLink="/jobs" routerLinkActive="active">Jobs</a>
        </nav>
        <router-outlet></router-outlet>
    `,
})
export class AppComponent  { title = 'Triton'; } 
