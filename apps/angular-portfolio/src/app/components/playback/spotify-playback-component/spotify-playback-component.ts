import {Component, inject, OnDestroy, OnInit, signal} from '@angular/core';
import {MeService, GetNowPlaying200Response} from '@api/generated-sdk';
import {interval, Subscription, switchMap, timer} from 'rxjs';
import {CommonModule} from '@angular/common';
import { LucideAngularModule, X } from 'lucide-angular';

@Component({
  selector: 'app-spotify-playback-component',
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './spotify-playback-component.html',
  styleUrl: './spotify-playback-component.scss',
})
export class SpotifyPlaybackComponent implements OnInit, OnDestroy {
  private readonly meService = inject(MeService);
  protected readonly playback = signal<GetNowPlaying200Response | null>(null);
  protected readonly currentTime = signal<number>(0);
  protected readonly progress = signal<number>(0);
  protected readonly isClosed = signal<boolean>(false);
  protected readonly X = X;

  private progressSub?: Subscription;

  ngOnInit(): void {
    this.startPolling();
  }

  ngOnDestroy(): void {
    this.progressSub?.unsubscribe();
  }

  private startPolling(): void {
    this.fetchPlayback();
  }

  private fetchPlayback(): void {
    this.meService.getNowPlaying().subscribe({
      next: (data) => {
        this.playback.set(data);
        if (data && data.trackType !== 'unknown') {
          this.currentTime.set(data.duration);
          this.updateProgress(data);
          this.scheduleNextFetch(data);
        } else {
          this.playback.set(null);
          this.progressSub?.unsubscribe();
        }
      },
      error: () => {
        this.playback.set(null);
        this.progressSub?.unsubscribe();
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

  private scheduleNextFetch(data: GetNowPlaying200Response): void {
    const remainingTime = data.totalDuration - data.duration;
    const delay = remainingTime + 10000;

    timer(delay).subscribe(() => this.fetchPlayback());
  }

  protected formatTime(ms: number): string {
    const totalSeconds = Math.floor(ms / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  protected close(): void {
    this.isClosed.set(true);
  }
}
