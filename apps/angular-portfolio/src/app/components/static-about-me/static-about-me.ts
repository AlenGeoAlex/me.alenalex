import {Component, inject, signal} from '@angular/core';
import {DomSanitizer, SafeHtml} from '@angular/platform-browser';
import {CommonModule} from '@angular/common';
import {AboutCardComponent} from "../about-bubble/about-bubble";



@Component({
  selector: 'app-static-about-me',
  standalone: true,
  imports: [CommonModule, AboutCardComponent],
  templateUrl: './static-about-me.html',
  styleUrl: './static-about-me.scss',
})
export class StaticAboutMe {
  visibleSections: boolean[] = [false, false, false, false];

  ngOnInit() {
    this.setupScrollObserver();
  }

  private setupScrollObserver() {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            const index = parseInt(entry.target.getAttribute('data-index') || '0');
            this.visibleSections[index] = true;
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
