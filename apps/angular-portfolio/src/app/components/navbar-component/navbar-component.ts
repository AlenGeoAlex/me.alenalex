import {Component, HostListener, inject, output, signal, ViewContainerRef} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import {BookOpen, Github, Home, LucideAngularModule, Mail, Menu, User, X} from 'lucide-angular';


@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, LucideAngularModule],
  templateUrl: './navbar-component.html'
})
export class NavbarComponent {
  isScrolled = signal(false);
  isMobileMenuOpen = signal(false);

  public contactMeRequest = output();

  readonly Home = Home;
  readonly User = User;
  readonly BookOpen = BookOpen;
  readonly Mail = Mail;
  readonly Menu = Menu;
  readonly X = X;

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.isScrolled.set(window.scrollY > 50);
  }

  toggleMobileMenu() {
    this.isMobileMenuOpen.set(!this.isMobileMenuOpen());
  }

  closeMobileMenu() {
    this.isMobileMenuOpen.set(false);
  }

  protected scrollToSection(sectionId: string) {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
      this.closeMobileMenu();
    }
  }

  protected routeToBlog() {
    window.open("https://blog.alenalex.me", "_blank")
  }

  protected onContactMe() {
    this.contactMeRequest.emit();
  }

  protected routeToGithub(){
    window.open("https://github.com/AlenGeoAlex/me.alenalex", "_blank")
  }

  protected readonly Github = Github;
}
