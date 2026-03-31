import {Component, effect, inject, signal} from '@angular/core';
import { CommonModule } from '@angular/common';
import {PostBlockEditorPreview} from '@components/admin/editor/post-block-editor-preview/post-block-editor-preview';
import {PostEditorNavbar, PostEditorTab} from '@components/admin/editor/post-editor-navbar/post-editor-navbar';
import {ActivatedRoute, Router} from '@angular/router';
import {HotToastService} from '@ngxpert/hot-toast';
import {PostService} from '@services/api/generated-sdk';
import {rxResource} from '@angular/core/rxjs-interop';
import {catchError} from 'rxjs';

@Component({
  selector: 'bloggi-post-editor',
  imports: [PostEditorNavbar, CommonModule, PostBlockEditorPreview],
  templateUrl: './post-editor.html',
  styleUrl: './post-editor.scss',
})
export class PostEditor {
  protected activeTab = signal<PostEditorTab>('editor');
  protected isEditorDirty = signal(false);
  protected isPreviewDirty = signal(false);
  protected isMetadataDirty = signal(false);

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toastService = inject(HotToastService);

  protected readonly postId = signal<string>('');
  private readonly postService = inject(PostService);
  private readonly postResource = rxResource(
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
}
