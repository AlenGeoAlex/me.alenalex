import { Component } from '@angular/core';
import {animate} from 'motion';
import {Github, Linkedin, LucideAngularModule, Mail} from 'lucide-angular';

@Component({
  selector: 'app-home-page',
  imports: [
    LucideAngularModule
  ],
  templateUrl: './home-page.html',
})
export class HomePage {

  ngOnInit() {
    this.initializeAnimations();
  }

  initializeAnimations() {
    // Animate hero text on load
    animate(
      '.hero-greeting',
      { opacity: [0, 1], y: [30, 0] },
      { duration: 0.8, delay: 0.2 }
    );

    animate(
      '.hero-title',
      { opacity: [0, 1], y: [30, 0] },
      { duration: 0.8, delay: 0.4 }
    );

    animate(
      '.hero-subtitle',
      { opacity: [0, 1], y: [30, 0] },
      { duration: 0.8, delay: 0.6 }
    );

    animate(
      '.social-icons',
      { opacity: [0, 1], scale: [0.8, 1] },
      { duration: 0.6, delay: 0.8 }
    );

    animate(
      '.cta-button',
      { opacity: [0, 1], y: [20, 0] },
      { duration: 0.6, delay: 1 }
    );

    animate(
      '.scroll-indicator',
      { y: [0, 8, 0] },
      { duration: 2, repeat: Infinity, ease: "easeIn" }
    );
  }

  scrollToAbout() {
    const element = document.getElementById('about');
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  }

  downloadCV() {
    window.open('/assets/your-cv.pdf', '_blank');
  }


  protected readonly Linkedin = Linkedin;
  protected readonly Github = Github;
  protected readonly Mail = Mail;
}
