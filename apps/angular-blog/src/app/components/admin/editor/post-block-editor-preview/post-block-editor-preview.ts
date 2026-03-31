import {Component, input} from '@angular/core';
import {SplitterModule} from 'primeng/splitter';
import {PrimeTemplate} from 'primeng/api';
import {
  PostBlockEditor
} from '@components/admin/editor/post-block-editor-preview/lib/post-block-editor/post-block-editor';
import {
  PostBlockPreview
} from '@components/admin/editor/post-block-editor-preview/lib/post-block-preview/post-block-preview';
@Component({
  selector: 'bloggi-post-block-editor-preview',
  imports: [
    SplitterModule,
    PostBlockEditor,
    PostBlockPreview,
    PrimeTemplate
  ],
  templateUrl: './post-block-editor-preview.html',
  styleUrl: './post-block-editor-preview.scss',
})
export class PostBlockEditorPreview {

  public readonly postId = input.required<string>();

}
