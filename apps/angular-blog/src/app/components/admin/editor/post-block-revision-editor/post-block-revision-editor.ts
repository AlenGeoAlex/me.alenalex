import {Component, inject, input, signal} from '@angular/core';
import {TableModule} from "primeng/table";
import {Button} from 'primeng/button';
import {
  BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse,
  PostService
} from '@services/api/generated-sdk';
import {HotToastService} from '@ngxpert/hot-toast';
import {rxResource} from '@angular/core/rxjs-interop';
import {DatePipe} from '@angular/common';
import {Tooltip} from 'primeng/tooltip';
import {asProblemDetailsAsync} from '@utils/http-utils';
import {ConfirmationService} from 'primeng/api';

@Component({
  selector: 'bloggi-post-block-revision-editor',
  imports: [
    TableModule,
    Button,
    DatePipe,
    Tooltip
  ],
  templateUrl: './post-block-revision-editor.html',
  styleUrl: './post-block-revision-editor.scss',
})
export class PostBlockRevisionEditor {

  public readonly post = input.required<BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse | undefined>();
  private readonly toastService = inject(HotToastService);
  private readonly postService = inject(PostService);
  protected readonly isRevisionCreationLoading = signal(false);
  private readonly confirmationService = inject(ConfirmationService);

  protected readonly revisionResource = rxResource({
    params: () => ({
      postId: this.post()!.id!
    }),
    stream: ({params}) => {
      return this.postService.listRevisions(params.postId)

    }
  })

  protected createRevision() {
    this.isRevisionCreationLoading.set(true);
    this.postService.createRevision(this.post()!.id!)
      .subscribe({
        next: () => {
          this.revisionResource.reload();
          this.toastService.success('Revision created successfully');
        },
        error: (error) => {
          asProblemDetailsAsync(error).then(problemDetails => {
            this.toastService.error(problemDetails.detail);
          })
          console.error('Error creating revision:', error);
          this.isRevisionCreationLoading.set(false);
        },
        complete: () => {
          this.isRevisionCreationLoading.set(false);
        }
      })
  }

  protected viewRevision(revision: any) {

  }

  protected deleteRevision(event: Event ,revision: any) {
    this.confirmationService.confirm({
      header: 'Danger Zone (Deletion)',
      target: event.target as HTMLElement,
      message: 'Are you sure you want to delete this revision? This action cannot be undone.',
      icon: 'pi pi-warning-triangle',
      rejectLabel: 'Cancel',
      acceptLabel: 'Delete',
      rejectButtonProps: {
        label: 'Cancel',
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: 'Delete',
        severity: 'danger'
      },
      accept: () => {
        this.postService.deleteRevision(this.post()!.id!, revision.id!)
          .subscribe({
            next: () => {
              this.revisionResource.reload();
            },
            error: (error) => {
              asProblemDetailsAsync(error)
                .then((pd) => {
                  this.toastService.error(pd.detail);
                })
            }
          });
      },
    })
  }

  protected newRevisionFrom(event: Event ,revision: any) {
    this.confirmationService.confirm({
      header: 'Danger Zone',
      target: event.target as HTMLElement,
      message: 'Are you sure you want to create a new revision from this revision? This will overwrite the current block data with the data from this revision.',
      icon: 'pi pi-info-circle',
      rejectLabel: 'Cancel',
      rejectButtonProps: {
        label: 'Cancel',
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: 'Proceed',
        severity: 'danger'
      },
      accept: () => {
        this.postService.fromExistingRevision(this.post()!.id!, revision.id!, true)
          .subscribe({
            next: value => {
              this.revisionResource.reload();
              this.toastService.success('Revision rolled out successfully. Please wait for the changes to be reflected in the live site in few seconds.');
              this.confirmationService.close();
            },
            error: (error) => {
              asProblemDetailsAsync(error).then(problemDetails => {
                this.toastService.error(problemDetails.detail);
              })
              console.error('Error creating revision:', error);
            }
          })
      },
    })
  }

  protected publishRevision(revision: any) {

  }
}
