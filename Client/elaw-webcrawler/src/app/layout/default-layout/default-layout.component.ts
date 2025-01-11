import { Component } from '@angular/core';
import { DefaultHeaderComponent } from "./default-header/default-header.component";
import { DefaultNavComponent } from "./default-nav/default-nav.component";
import { DefaultFooterComponent } from "./default-footer/default-footer.component";
import { RouterOutlet } from '@angular/router';
import { NgxSpinnerComponent, NgxSpinnerService } from "ngx-spinner";

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    DefaultHeaderComponent,
    DefaultFooterComponent,
    RouterOutlet,
    NgxSpinnerComponent
],
  templateUrl: './default-layout.component.html',
  styleUrl: './default-layout.component.scss'
})
export class DefaultLayoutComponent {

}
