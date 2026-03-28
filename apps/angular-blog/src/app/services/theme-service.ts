import {DestroyRef, inject, Injectable} from '@angular/core';
import {BehaviorSubject, } from 'rxjs';
import {ConfigKeys} from '@constants/config-keys';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {

  private readonly destroyRef = inject(DestroyRef);

  private readonly darkModeSubject = new BehaviorSubject<boolean>(
    localStorage.getItem(ConfigKeys.DARK_MODE_PERSISTENCE) === 'true'
  );

  public get darkMode$() {
    return this.darkModeSubject.asObservable();
  }

  constructor() {
    this.darkMode$
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(enabled => {
        document.documentElement.classList.toggle('dark', enabled);
      });
  }

  public get darkMode(): boolean {
    return this.darkModeSubject.value;
  }

  public toggleDarkMode(): void {
    this.setDarkMode(!this.darkModeSubject.value);
  }

  public setDarkMode(enabled: boolean): void {
    this.darkModeSubject.next(enabled);
    localStorage.setItem(ConfigKeys.DARK_MODE_PERSISTENCE, String(enabled));

    if (enabled) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }

}
