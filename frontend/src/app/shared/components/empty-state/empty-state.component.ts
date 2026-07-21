import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './empty-state.component.html',
  styleUrl: './empty-state.component.css'
})
export class EmptyStateComponent {

  @Input({ required: true })
  title!: string;

  @Input({ required: true })
  message!: string;

  @Input()
  icon = 'inbox';

  @Input()
  actionText = '';

  @Output()
  action = new EventEmitter<void>();

}