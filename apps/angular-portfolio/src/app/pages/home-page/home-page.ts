import {Component, inject, signal} from '@angular/core';
import {animate} from 'motion';
import {Github, Linkedin, LucideAngularModule, LucideIconData, Mail} from 'lucide-angular';
import {MeService} from '@api/generated-sdk';
import {HotToastService} from '@ngxpert/hot-toast';

@Component({
  selector: 'app-home-page',
  imports: [
    LucideAngularModule
  ],
  templateUrl: './home-page.html',
})
export class HomePage {

  protected readonly isCVLoading = signal(false);
  private readonly toastService = inject(HotToastService);
  private readonly meService = inject(MeService);

  ngOnInit() {
    this.initializeAnimations();
  }

  initializeAnimations() {
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
    this.isCVLoading.set(true);
    this.meService.getCvGet()
      .subscribe({
        next: (data) => {
          this.isCVLoading.set(false);
          window.open(data.signedUrl, '_blank');
        },
        error: (error) => {
          console.error(error);
          this.toastService.error('Failed to download CV');
          this.isCVLoading.set(false);
        }
      })
  }


  protected readonly Linkedin = Linkedin;
  protected readonly Github = Github;
  protected readonly Mail = Mail;
  protected readonly DiscordIcon : LucideIconData = [
    [
      "path",
      {
        "d": "M15.995 15.33C14.812 15.33 13.838 14.245 13.838 12.911C13.838 11.578 14.793 10.492 15.995 10.492C17.205 10.492 18.171 11.588 18.152 12.912C18.152 14.245 17.206 15.33 15.995 15.33Z",
        "key": "17qv9d"
      }
    ],
    [
      "path",
      {
        "d": "M20.317 4.37A19.791 19.791 0 0 0 15.432 2.855A0.074 0.074 0 0 0 15.353 2.892C15.143 3.267 14.909 3.756 14.745 4.142A18.27 18.27 0 0 0 9.258 4.142A12.64 12.64 0 0 0 8.641 2.892A0.077 0.077 0 0 0 8.562 2.855A19.736 19.736 0 0 0 3.677 4.37A0.07 0.07 0 0 0 3.645 4.397C0.533 9.046 -0.32 13.58 0.099 18.057A0.082 0.082 0 0 0 0.13 18.114A19.9 19.9 0 0 0 6.123 21.144A0.078 0.078 0 0 0 6.207 21.116A14.09 14.09 0 0 0 7.433 19.122A0.076 0.076 0 0 0 7.392 19.016A13.107 13.107 0 0 1 5.52 18.124A0.077 0.077 0 0 1 5.512 17.996A10.2 10.2 0 0 0 5.884 17.704A0.074 0.074 0 0 1 5.961 17.694C9.889 19.487 14.141 19.487 18.023 17.694A0.074 0.074 0 0 1 18.101 17.704C18.221 17.802 18.347 17.902 18.474 17.996A0.077 0.077 0 0 1 18.468 18.123A12.299 12.299 0 0 1 16.595 19.015A0.077 0.077 0 0 0 16.554 19.122C16.914 19.82 17.326 20.484 17.779 21.115A0.076 0.076 0 0 0 17.863 21.143A19.839 19.839 0 0 0 23.865 18.113A0.077 0.077 0 0 0 23.897 18.059C24.397 12.882 23.059 8.385 20.348 4.399A0.061 0.061 0 0 0 20.317 4.369Z",
        "key": "1s6jvi"
      }
    ],
    [
      "path",
      {
        "d": "M8.02 15.33C6.837 15.33 5.863 14.245 5.863 12.911C5.863 11.578 6.819 10.492 8.02 10.492C9.23 10.492 10.196 11.588 10.177 12.912C10.177 14.245 9.221 15.33 8.02 15.33Z",
        "key": "1oj8s6"
      }
    ]
  ];


}
