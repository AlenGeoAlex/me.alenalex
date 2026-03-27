import {Component, signal} from '@angular/core';

@Component({
  selector: 'terminal-help-bar',
  imports: [],
  templateUrl: './help-bar.html',
  styleUrl: './help-bar.css',
})
export class HelpBar {
  protected isExpanded = signal(false);

  public toggleHelp() {
    this.isExpanded.update(v => !v);
  }
}
