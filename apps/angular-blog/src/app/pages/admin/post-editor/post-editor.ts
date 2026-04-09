import {Component, effect, inject, signal} from '@angular/core';
import { CommonModule } from '@angular/common';
import {PostBlockEditorPreview} from '@components/admin/editor/post-block-editor-preview/post-block-editor-preview';
import {PostEditorNavbar, PostEditorTab} from '@components/admin/editor/post-editor-navbar/post-editor-navbar';
import {ActivatedRoute, Router} from '@angular/router';
import {HotToastService} from '@ngxpert/hot-toast';
import {PostService} from '@services/api/generated-sdk';
import {rxResource} from '@angular/core/rxjs-interop';
import {catchError} from 'rxjs';
import {PostBasicEditor} from '@components/admin/editor/post-basic-editor/post-basic-editor';
import {PostMetadataEditor} from '@components/admin/editor/post-metadata-editor/post-metadata-editor';
import {PostFileEditor} from '@components/admin/editor/post-file-editor/post-file-editor';
import {PostBlockRevisionEditor} from '@components/admin/editor/post-block-revision-editor/post-block-revision-editor';

@Component({
  selector: 'bloggi-post-editor',
  imports: [PostEditorNavbar, CommonModule, PostBlockEditorPreview, PostBasicEditor, PostMetadataEditor, PostFileEditor, PostBlockRevisionEditor],
  templateUrl: './post-editor.html',
  styleUrl: './post-editor.scss',
})
export class PostEditor {
  protected activeTab = signal<PostEditorTab>('basic');
  protected isEditorDirty = signal(false);

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toastService = inject(HotToastService);

  protected readonly postId = signal<string>('');
  private readonly postService = inject(PostService);
  protected readonly postResource = rxResource(
    {
      params: () => ({
        postId: this.postId(),
      }),
      stream: (params) => {
        return this.postService.getPost(
          params.params.postId,
          ["Author", 'Tags']
        )
      }
    }
  )

  constructor() {
    effect(() => {
      const postStatus = this.postResource.status();
      if(postStatus === 'error'){
        this.toastService.error('Failed to fetch post');
        return;
      }
    })


    const postId = this.route.snapshot.paramMap.get('postId');
    if(!postId){
      this.toastService.error('Invalid post ID');
      setTimeout(() => {
        this.router.navigate(['/admin'])
          .catch(console.error)
      }, 2500)
      return;
    }

    this.postId.set(postId);
  }

  protected onTabChange(tab: PostEditorTab) {
    this.activeTab.set(tab);
  }

  protected onEditorDirty($event: boolean) {
    this.isEditorDirty.set($event);
  }

  protected onEditorLastMutatedOn($event: Date | undefined) {
    if($event){
      this.isEditorDirty.set(true);
    }
  }
}
