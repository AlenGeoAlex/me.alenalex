import {Component, inject, OnDestroy, OnInit, signal} from '@angular/core';
import {MeService, GetNowPlaying200Response} from '@api/generated-sdk';
import {interval, Subscription, timer} from 'rxjs';
import {CommonModule} from '@angular/common';

@Component({
  selector: 'app-spotify-playback-component',
  imports: [CommonModule],
  templateUrl: './spotify-playback-component.html',
  styleUrl: './spotify-playback-component.scss',
})
export class SpotifyPlaybackComponent implements OnInit, OnDestroy {
  private readonly meService = inject(MeService);
  protected readonly playback = signal<GetNowPlaying200Response | null>(null);
  protected readonly currentTime = signal<number>(0);
  protected readonly progress = signal<number>(0);

  private pollingSub?: Subscription;
  private progressSub?: Subscription;

  ngOnInit(): void {
    this.fetchPlayback();
  }

  ngOnDestroy(): void {
    this.pollingSub?.unsubscribe();
    this.progressSub?.unsubscribe();
  }

  private fetchPlayback(): void {
    this.pollingSub?.unsubscribe();
    this.meService.getNowPlaying().subscribe({
      next: (data) => {
        this.playback.set(data);
        if (data && data.trackType !== 'unknown') {
          this.currentTime.set(data.duration);
          this.updateProgress(data);
          this.scheduleNextFetch(data, true);
        } else {
          this.playback.set(null);
          this.progressSub?.unsubscribe();
          this.scheduleNextFetch(null, false);
        }
      },
      error: () => {
        this.playback.set(null);
        this.progressSub?.unsubscribe();
        this.scheduleNextFetch(null, false);
      }
    });
  }

  private updateProgress(data: GetNowPlaying200Response): void {
    this.progressSub?.unsubscribe();

    this.progressSub = interval(1000).subscribe(() => {
      const nextTime = this.currentTime() + 1000;
      if (nextTime <= data.totalDuration) {
        this.currentTime.set(nextTime);
        this.progress.set((nextTime / data.totalDuration) * 100);
      } else {
        this.progress.set(100);
        this.progressSub?.unsubscribe();
      }
    });
  }

  private scheduleNextFetch(data: GetNowPlaying200Response | null, isPlaying: boolean): void {
    this.pollingSub?.unsubscribe();

    let delay = 30000;
    if (isPlaying && data) {
      const remainingTime = data.totalDuration - data.duration;
      // poll after song ends + 5 sec
      delay = Math.max(remainingTime + 5000, 5000); // Ensure at least 5s delay
    }

    this.pollingSub = timer(delay).subscribe(() => this.fetchPlayback());
  }

  protected formatTime(ms: number): string {
    const totalSeconds = Math.floor(ms / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }
}
