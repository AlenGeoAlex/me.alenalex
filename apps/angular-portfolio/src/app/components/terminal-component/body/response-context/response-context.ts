import {Component, computed, inject, input} from '@angular/core';
import {DomSanitizer} from '@angular/platform-browser';
import {marked} from 'marked';

export interface IResponseContextInput {
  contentType: 'text/plain' | 'text/html' | 'application/pdf' | 'image/png' | 'image/jpeg' | 'video/mp4' | 'audio/mpeg' | 'text/markdown';
  content: string;
}

@Component({
  selector: 'terminal-body-response-context',
  imports: [],
  templateUrl: './response-context.html',
  styleUrl: './response-context.scss',
})
export class ResponseContext {
  public id = input.required<string | number>();
  public data = input.required<IResponseContextInput>();
  private readonly sanitizer = inject(DomSanitizer);
  protected isText = computed(() => this.data().contentType === 'text/plain');
  protected isHtml = computed(() => this.data().contentType === 'text/html');
  protected isMarkdown = computed(() => this.data().contentType === 'text/markdown');
  protected isImage = computed(() =>  this.data().contentType === 'image/png' || this.data().contentType === 'image/jpeg');
  protected isPdf = computed(() => this.data().contentType === 'application/pdf');
  protected isAudio = computed(() => this.data().contentType === 'audio/mpeg');
  protected textLines = computed(() => this.data().content.split('\n'));
  protected isVideo = computed(() => this.data().contentType === 'video/mp4');
  protected markdownHtml = computed(() => {
    const html = marked.parse(this.data().content);
    return this.sanitizer.sanitize(1, html) || '';
  });

  protected mediaUrl = computed(() =>
    this.sanitizer.bypassSecurityTrustResourceUrl(this.data().content)
  );
  protected safeHtml = computed(() => {
    return this.sanitizer.sanitize(1, this.data().content) || '';
  });
  constructor() {}

}
