import {DestroyRef, inject, Injectable} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {BehaviorSubject} from 'rxjs';
import {ConfigKeys} from '@constants/config-keys.constants';

@Injectable({
  providedIn: 'root',
})
export class Appearance {
  private readonly destroyRef = inject(DestroyRef);
  private static readonly DARK_MODE_TOGGLE_CLASS = 'blg-alex-dark';

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
        document.documentElement.classList.toggle(Appearance.DARK_MODE_TOGGLE_CLASS, enabled);
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
      document.documentElement.classList.add(Appearance.DARK_MODE_TOGGLE_CLASS);
    } else {
      document.documentElement.classList.remove(Appearance.DARK_MODE_TOGGLE_CLASS);
    }
  }
}
