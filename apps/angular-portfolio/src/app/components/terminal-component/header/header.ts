import {Component, input, model, output, signal} from '@angular/core';
import {LucideAngularModule, Maximize2, Minus, X} from 'lucide-angular';

@Component({
  selector: 'terminal-header',
  imports: [
    LucideAngularModule
  ],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  protected readonly X = X;
  protected readonly Minus = Minus;
  protected readonly Maximize2 = Maximize2;

  public title = input();
  public close = output();
}


