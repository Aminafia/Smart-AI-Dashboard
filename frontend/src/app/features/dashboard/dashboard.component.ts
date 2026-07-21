import { Component } from '@angular/core';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { PageCardComponent } from '../../shared/components/page-card/page-card.component';

@Component({
  selector: 'app-dashboard',
  imports: [
    PageHeaderComponent,
    PageCardComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent {

}
