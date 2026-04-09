import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

export type PostEditorTab = 'basic' | 'editor' | 'metadata' | 'files' | 'series' | 'revisions' | 'preview';
@Component({
  selector: 'bloggi-post-editor-navbar',
  standalone: true,
  imports: [CommonModule, ButtonModule, TooltipModule],
  templateUrl: './post-editor-navbar.html',
  styleUrl: './post-editor-navbar.scss',
})
export class PostEditorNavbar {
  activeTab = input.required<PostEditorTab>();
  editorDirty = input<boolean>(false);
  previewDirty = input<boolean>(false);
  metadataDirty = input<boolean>(false);

  tabChange = output<PostEditorTab>();
  protected tabClass(tab: PostEditorTab): string {
    const base = [
      'flex items-center gap-1.5',
      'h-10 px-4',
      'text-sm border-b-2 -mb-px',
      'whitespace-nowrap cursor-pointer bg-transparent border-x-0 border-t-0',
      'transition-colors duration-150',
    ].join(' ');

    const active = 'border-b-amber-600 dark:border-b-amber-500 text-amber-700 dark:text-amber-400 font-medium';
    const inactive = 'border-b-transparent text-[var(--ink-3)] hover:text-[var(--ink)] hover:border-b-[var(--border-2)] font-normal';

    return `${base} ${this.activeTab() === tab ? active : inactive}`;
  }

  protected onTabClick(metadata: string) {
    this.tabChange.emit(metadata as PostEditorTab);
  }
}
