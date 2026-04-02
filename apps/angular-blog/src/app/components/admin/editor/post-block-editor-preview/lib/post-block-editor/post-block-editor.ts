import {Component, effect, inject, input, signal, viewChild} from '@angular/core';
import EditorJS from '@editorjs/editorjs';
import {EditorService} from '@services/editor.service';

@Component({
  selector: 'bloggi-post-block-editor',
  imports: [],
  templateUrl: './post-block-editor.html',
  styleUrl: './post-block-editor.scss',
})
export class PostBlockEditor {

  public readonly postId = input.required<string>();
  protected readonly editorInitialized = signal(false);
  private readonly editorComponent = viewChild<HTMLDivElement>('editor')
  private _editor?: EditorJS ;
  private readonly editorService = inject(EditorService);

  constructor() {
    effect(() => {
      const postId = this.postId();
      this._editor = this.editorService.createEngine(this.postId());
      setInterval(() => {
        this.editor.save().then(data => {
          console.log(data)
        })
      }, 5000)
    })
  }

  public get editor() {
    return this._editor!;
  }
}
