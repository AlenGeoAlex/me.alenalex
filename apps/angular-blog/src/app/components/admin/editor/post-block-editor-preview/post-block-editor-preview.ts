import {CommonModule} from '@angular/common';
import {Component, effect, inject, input, model, output, signal} from '@angular/core';
import {SplitterModule} from 'primeng/splitter';
import {PrimeTemplate} from 'primeng/api';
import {
  PostBlockEditor
} from '@components/admin/editor/post-block-editor-preview/lib/post-block-editor/post-block-editor';
import {
  PostBlockPreview
} from '@components/admin/editor/post-block-editor-preview/lib/post-block-preview/post-block-preview';
import {
  BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse,
  BloggiBackendApiWebFeaturesPostEndpointsPreviewRenderRenderResponse, BloggiBackendEditorJSCoreBlockTypes,
  PostService
} from '@services/api/generated-sdk';
import {OutputData} from '@editorjs/editorjs';
import {HotToastService} from '@ngxpert/hot-toast';
import {asProblemDetailsAsync} from '@utils/http-utils';
@Component({
  selector: 'bloggi-post-block-editor-preview',
  imports: [
    CommonModule,
    SplitterModule,
    PostBlockEditor,
    PostBlockPreview
  ],
  templateUrl: './post-block-editor-preview.html',
  styleUrl: './post-block-editor-preview.scss',
})
export class PostBlockEditorPreview {

  public readonly post = input.required<BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse | undefined>();
  protected readonly editorInitialized = signal(false);
  protected readonly editorLastMutatedOn = model<Date | undefined>();
  public readonly editorData = model<OutputData | undefined>();
  public readonly editorPreview = model<BloggiBackendApiWebFeaturesPostEndpointsPreviewRenderRenderResponse | undefined>();
  public readonly editorDirty = signal(false);
  private readonly toastService = inject(HotToastService);

  private readonly postService = inject(PostService);
  private debounceTimer: ReturnType<typeof setTimeout> | undefined;

  constructor() {
    effect(() => {
      const editorMutation = this.editorLastMutatedOn();
      const editorData = this.editorData();

      if (!editorData) return;
      clearTimeout(this.debounceTimer);

      if(this.editorData() === undefined) return;

      this.debounceTimer = setTimeout(() => {
        console.log('Saving post block', this.post()!.id, editorData)
        this.postService.upsertPostBlock(this.post()!.id!, {
          editorJsData: {
            version: editorData!.version!,
            time: editorData!.time!,
            blocks: editorData!.blocks.map(x => {
              return {
                id: x.id,
                type: x.type as BloggiBackendEditorJSCoreBlockTypes,
                data: x.data
              }
            })
          }
        })
          .subscribe({
            next: () => {
              this.editorDirty.set(false);
              var loading = this.toastService.loading('Rendering post preview...');
              this.postService.preview(this.post()!.id!)
                .pipe(
                )
                .subscribe({
                  next: (res) => {
                    this.editorPreview.set(res);
                    loading.close()
                  },
                  error: (err) => {
                    loading.close()
                    asProblemDetailsAsync(err)
                      .then(problemDetails => {
                        this.toastService.error(problemDetails.detail);
                      })
                  }
                })
            },
            error: (err) => {
              console.error('Error saving post block', err);
              this.toastService.error('Failed to save post block');
            }
          })
      }, 3000);
    })

    effect(() => {
      const post = this.post();
      if(!post) return;

      const postId = post.id;
      if(!postId) return;
      this.postService.getPostBlocksByPostId(postId)
        .subscribe({
          next: (blocks) => {
            if (!blocks.outputData!.blocks.length) {
              blocks.outputData!.blocks = []
            }

            this.editorData.set(blocks.outputData as OutputData);
          },
          error: (err) => {
            console.error('Error fetching post blocks', err);
            this.toastService.error('Failed to fetch post blocks');
          }
        })
    })
  }

  protected onEditorReady($event: boolean) {
    this.editorInitialized.set($event);
  }

  protected onEditorLastMutatedOn($event: Date | undefined) {
    this.editorLastMutatedOn.set($event);
  }
}
