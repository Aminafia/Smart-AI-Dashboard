import { Component } from '@angular/core';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { PageCardComponent } from '../../shared/components/page-card/page-card.component';
import { MatIconModule } from '@angular/material/icon';
@Component({
  selector: 'app-dashboard',
  imports: [
    PageHeaderComponent,
    PageCardComponent,
    MatIconModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent {

}
