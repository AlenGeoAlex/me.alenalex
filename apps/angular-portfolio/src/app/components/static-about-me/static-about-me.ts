import {Component, signal} from '@angular/core';
import {CommonModule} from '@angular/common';


@Component({
  selector: 'app-static-about-me',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './static-about-me.html',
  styleUrl: './static-about-me.scss',
})
export class StaticAboutMe {
  visibleSections= signal([false, false, false, false]);

  ngOnInit() {
    this.setupScrollObserver();
  }

  private setupScrollObserver() {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            const index = parseInt(entry.target.getAttribute('data-index') || '0');
            this.visibleSections.update(x => {
              x[index] = true;
              return x
            })
          }
        });
      },
      { threshold: 0.2 }
    );

    setTimeout(() => {
      const sections = document.querySelectorAll('.fade-in');
      sections.forEach((section, index) => {
        section.setAttribute('data-index', index.toString());
        observer.observe(section);
      });
    }, 100);
  }
}
