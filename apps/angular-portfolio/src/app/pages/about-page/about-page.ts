import { Component } from '@angular/core';
import {TerminalComponent} from '../../components/terminal-component/terminal-component';

@Component({
  selector: 'app-about-page',
  templateUrl: './about-page.html',
  imports: [
    TerminalComponent
  ]
})
export class AboutPage {
  currentSection = 0;
  typingComplete = false;

  sections: TerminalSection[] = [
    {
      id: 'intro',
      title: 'Introduction',
      command: 'cat about.txt',
      content: [
        'Hi! I\'m a passionate software engineer with X years of experience.',
        'I love building scalable applications and solving complex problems.'
      ]
    },
    {
      id: 'education',
      title: 'Education',
      command: 'cat education.txt',
      content: [
        'Bachelor of Science in Computer Science',
        'University Name | Year - Year',
        'Relevant Coursework: Data Structures, Algorithms, Web Development'
      ]
    },
    {
      id: 'interests',
      title: 'Interests & Hobbies',
      command: 'cat interests.txt',
      content: [
        'Coding & Building Projects',
        'Reading Tech Blogs & Books',
        'Contributing to Open Source',
        'Photography & Travel',
        'Gaming & Chess'
      ]
    },
    {
      id: 'skills',
      title: 'Technical Skills',
      command: 'cat skills.txt',
      content: [
        'Languages: JavaScript, TypeScript, Python, Java',
        'Frontend: Angular, React, Vue.js, Tailwind CSS',
        'Backend: Node.js, Express, NestJS, Django',
        'Database: PostgreSQL, MongoDB, Redis',
        'Tools: Git, Docker, AWS, CI/CD'
      ]
    }
  ];

  ngOnInit() {
    // Initialize with first section visible
  }

  ngAfterViewInit() {

  }


  scrollToNextSection() {
    if (this.currentSection < this.sections.length - 1) {
      this.currentSection++;
      const nextElement = document.getElementById(`section-${this.currentSection}`);
      if (nextElement) {
        nextElement.scrollIntoView({ behavior: 'smooth' });
      }
    } else {
      // Scroll to contact section
      const contactElement = document.getElementById('contact');
      if (contactElement) {
        contactElement.scrollIntoView({ behavior: 'smooth' });
      }
    }
  }

  scrollToPreviousSection() {
    if (this.currentSection > 0) {
      this.currentSection--;
      const prevElement = document.getElementById(`section-${this.currentSection}`);
      if (prevElement) {
        prevElement.scrollIntoView({ behavior: 'smooth' });
      }
    }
  }

}
interface TerminalSection {
  id: string;
  title: string;
  command: string;
  content: string[];
}

