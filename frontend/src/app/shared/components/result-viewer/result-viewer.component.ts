import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

import { EmptyStateComponent } from '../empty-state/empty-state.component';

@Component({
  selector: 'app-result-viewer',
  standalone: true,
  imports: [
    CommonModule,
    EmptyStateComponent
  ],
  templateUrl: './result-viewer.component.html',
  styleUrl: './result-viewer.component.css'
})
export class ResultViewerComponent {

  @Input()
  html = '';

  @Input()
  status = '';

  @Input()
  error = '';

  @Input()
  emptyTitle = 'No Results Yet';

  @Input()
  emptyMessage = 'Run an AI operation to see the result here.';

  @Input()
  emptyIcon = 'auto_awesome';

}