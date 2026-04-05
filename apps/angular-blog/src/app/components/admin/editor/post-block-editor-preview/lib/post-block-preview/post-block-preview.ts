import {Component, inject, input, linkedSignal, signal} from '@angular/core';
import {BloggiBackendApiWebFeaturesPostEndpointsPreviewRenderRenderResponse} from '@services/api/generated-sdk';
import { PopoverModule } from 'primeng/popover';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import {DomSanitizer} from '@angular/platform-browser';

@Component({
  selector: 'bloggi-post-block-preview',
  imports: [
    ButtonModule,
    TooltipModule,
    PopoverModule,
    BadgeModule,
    ButtonModule,
    ScrollPanelModule,
    TagModule,
    TooltipModule,
  ],
  templateUrl: './post-block-preview.html',
  styleUrl: './post-block-preview.scss',
})
export class PostBlockPreview {

  private readonly sanitizer = inject(DomSanitizer);
  public readonly response = input.required<BloggiBackendApiWebFeaturesPostEndpointsPreviewRenderRenderResponse | undefined>();
  public readonly responseHtml = linkedSignal(() => {
    const response = this.response();
    if (!response?.html) return '';

    const binary = atob(response.html);
    const bytes = Uint8Array.from(binary, c => c.charCodeAt(0));
    const html = new TextDecoder('utf-8').decode(bytes);

    return this.sanitizer.bypassSecurityTrustHtml(html);
  });

  public readonly responseError = linkedSignal(() => {
    const response = this.response();
    if(!response) return [];
    return response.errors;
  })

  toggle($event: MouseEvent) {

  }
}
