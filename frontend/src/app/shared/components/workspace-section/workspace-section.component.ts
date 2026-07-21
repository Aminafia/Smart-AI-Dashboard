import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

import { MatDividerModule } from '@angular/material/divider';
import { PageCardComponent } from '../page-card/page-card.component';

@Component({
  selector: 'app-workspace-section',
  standalone: true,
  imports: [
    CommonModule,
    MatDividerModule,
    PageCardComponent
  ],
  templateUrl: './workspace-section.component.html',
  styleUrl: './workspace-section.component.css'
})
export class WorkspaceSectionComponent {

  @Input({ required: true })
  title!: string;

  @Input()
  subtitle = '';

}