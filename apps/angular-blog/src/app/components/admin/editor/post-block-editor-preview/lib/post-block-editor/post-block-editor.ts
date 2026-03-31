import {Component, inject, input, viewChild} from '@angular/core';
import EditorJS from '@editorjs/editorjs';
import {EditorService} from '@services/editor.service';

@Component({
  selector: 'bloggi-post-block-editor',
  imports: [],
  templateUrl: './post-block-editor.html',
  styleUrl: './post-block-editor.scss',
})
export class PostBlockEditor {

  readonly postId = input.required<string>();
  private readonly editorComponent = viewChild<HTMLDivElement>('editor')
  private readonly editor: EditorJS;
  private readonly editorService = inject(EditorService);

  constructor() {
    this.editor = this.editorService.createEngine(this.postId());
  }
}
