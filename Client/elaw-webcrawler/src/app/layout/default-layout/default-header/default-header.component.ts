import { Component } from '@angular/core';
import { DefaultNavComponent } from "../default-nav/default-nav.component";

@Component({
  selector: 'app-default-header',
  standalone: true,
  imports: [DefaultNavComponent],
  templateUrl: './default-header.component.html',
  styleUrl: './default-header.component.scss'
})
export class DefaultHeaderComponent {
  toggleMenu() {

  }

}
