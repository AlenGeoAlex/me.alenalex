import {Component, inject, OnDestroy, OnInit, signal} from '@angular/core';
import {MeService, GetNowPlaying200Response} from '@api/generated-sdk';
import {interval, Subscription, timer} from 'rxjs';
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
  private pollSub?: Subscription;

  ngOnInit(): void {
    this.fetchPlayback();
  }

  ngOnDestroy(): void {
    this.progressSub?.unsubscribe();
    this.pollSub?.unsubscribe();
  }

  private fetchPlayback(): void {
    this.meService.getNowPlaying().subscribe({
      next: (data) => {
        if (data && data.trackType !== 'unknown') {
          this.playback.set(data);
          this.currentTime.set(data.duration);
          this.updateProgress(data);
          this.scheduleNextFetch(data);
        } else {
          this.playback.set(null);
          this.progressSub?.unsubscribe();
          this.scheduleIdlePoll();
        }
      },
      error: () => {
        this.playback.set(null);
        this.progressSub?.unsubscribe();
        this.scheduleIdlePoll();
      }
    });
  }

  private scheduleNextFetch(data: GetNowPlaying200Response): void {
    this.pollSub?.unsubscribe();
    const remainingTime = data.totalDuration - data.duration;
    const delay = remainingTime + 50000;

    this.pollSub = timer(delay).subscribe(() => this.fetchPlayback());
  }

  private scheduleIdlePoll(): void {
    this.pollSub?.unsubscribe();
    this.pollSub = timer(30000).subscribe(() => this.fetchPlayback());
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
