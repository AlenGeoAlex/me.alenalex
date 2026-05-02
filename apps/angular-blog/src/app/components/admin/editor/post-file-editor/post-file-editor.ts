import {Component, inject, input, signal} from '@angular/core';
import {TableModule} from 'primeng/table';
import {
  BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse,
  PostService
} from '@services/api/generated-sdk';
import {HotToastService} from '@ngxpert/hot-toast';
import {HashService} from '@services/hash.service';
import {rxResource} from '@angular/core/rxjs-interop';
import {map, tap} from 'rxjs';
import {FilesizePipe} from '@pipes/filesize-pipe';
import {Button} from 'primeng/button';
import {Tooltip} from 'primeng/tooltip';
import {Badge} from 'primeng/badge';

@Component({
  selector: 'bloggi-post-file-editor',
  imports: [
    TableModule,
    FilesizePipe,
    Button,
    Tooltip,
    Badge
  ],
  templateUrl: './post-file-editor.html',
  styleUrl: './post-file-editor.scss',
})
export class PostFileEditor {

  public readonly post = input.required<BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse | undefined>();
  private readonly toastService = inject(HotToastService);
  private readonly postService = inject(PostService);
  protected readonly deletingIds = signal<Set<string>>(new Set());
  private readonly metadataResource = rxResource({
    params: () => ({
      postId: this.post()?.id!
    }),
    stream: ({ params}) => {
      return this.postService.getPostMeta(params.postId)
    }
  })
  protected readonly postFileResource = rxResource({
    params: () => ({
      postId: this.post()?.id!
    }),
    stream: ({ params}) => {
      return this.postService.getPostFiles(params.postId)
        .pipe(
          map(value => {
            const ogIdd = this.metadataResource.value()?.openGraphImageUrl || '';
            return value.files
                ?.map(x => {
                  return {
                    ...x,
                    isOg: ogIdd.includes(x.fileId!)
                  }
                })
              ?? []
          })
        )
    }
  })

  protected deleteFile(file: any) {
    this.deletingIds.update(ids => ids.add(file.id));
    this.postService.deleteFile(this.post()?.id!, file.fileId!)
      .subscribe({
        next: () => {
          this.postFileResource.reload();
          this.deletingIds.update(ids => {
            ids.delete(file.id)
            return ids;
          });
        },
        error: (err) => {
          console.error('Error deleting file', err);
          this.toastService.error('Failed to delete file');
          this.deletingIds.update(ids => {
            ids.delete(file.id)
            return ids;
          });
        }
      })
  }

  protected showPreview(file: any) {
    this.postService.getFile(this.post()?.id!, file.fileId!)
      .subscribe({
        next: (res) => {
          window.open(res.url, '_blank');
        },
        error: (err) => {
          console.error('Error fetching file', err);
          this.toastService.error('Failed to fetch file');
        }
      })
  }
}
