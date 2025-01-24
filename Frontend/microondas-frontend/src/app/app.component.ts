import { Component } from '@angular/core';
import { MicroondasComponent } from './components/microondas/microondas.component';

@Component({
  selector: 'app-root',
  imports: [MicroondasComponent],
  template:
  `
    <app-microondas />
  `
})
export class AppComponent {
  title = 'microondas-frontend';
}
