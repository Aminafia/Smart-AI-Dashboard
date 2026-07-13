import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { LoadingOverlayComponent } from './shared/components/loading-overlay/loading-overlay.component';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet, 
    LoadingOverlayComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent { }
