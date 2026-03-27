import {Component, DestroyRef, inject, signal} from '@angular/core';

@Component({
  selector: 'terminal-body-spinner',
  imports: [],
  template: `
    <span class="inline-block text-green-400">
      {{ frames[currentFrame()] }}
    </span>
  `,
  styles: [`
    :host {
      display: inline-block;
    }
  `],
})
export class Spinner {
  protected frames = ['⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏'];
  protected currentFrame = signal(0);
  private readonly intervalId?: number;
  private readonly destroy = inject(DestroyRef)
  constructor() {
    this.intervalId = window.setInterval(() => {
      this.currentFrame.set((this.currentFrame() + 1) % this.frames.length);
    }, 80);
    this.destroy.onDestroy(() => {
      if(!this.intervalId)
        return;

      clearInterval(this.intervalId);
    })
  }
}
