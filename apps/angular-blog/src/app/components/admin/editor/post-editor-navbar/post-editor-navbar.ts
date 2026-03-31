import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

export type PostEditorTab = 'editor' | 'preview' | 'metadata';

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

  protected onTabClick(tab: PostEditorTab) {
    this.tabChange.emit(tab);
  }
}
