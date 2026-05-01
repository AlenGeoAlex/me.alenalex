import {Component, inject, input, linkedSignal, signal} from '@angular/core';
import {Button} from 'primeng/button';
import {InputText} from 'primeng/inputtext';
import {Select} from 'primeng/select';
import {FormsModule} from '@angular/forms';
import {
  BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse,
  BloggiBackendApiWebFeaturesPostEndpointsPostUpdatePostMetaUpdatePostMetaRequest,
  PostService
} from '@services/api/generated-sdk';
import {HotToastService} from '@ngxpert/hot-toast';
import {rxResource} from '@angular/core/rxjs-interop';
import {disabled, form, FormField, readonly, validate} from '@angular/forms/signals';
import {HashService} from '@services/hash.service';
import {EditorService} from '@services/editor.service';

@Component({
  selector: 'bloggi-post-metadata-editor',
  imports: [
    Button,
    InputText,
    Select,
    FormsModule,
    FormField
  ],
  templateUrl: './post-metadata-editor.html',
  styleUrl: './post-metadata-editor.scss',
})
export class PostMetadataEditor {

  public readonly post = input.required<BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse | undefined>();
  private readonly toastService = inject(HotToastService);
  private readonly hashService = inject(HashService);
  private readonly postService = inject(PostService);
  private readonly editorService = inject(EditorService);
  protected readonly formSignal = linkedSignal(() => {
    const metadata = this.metadataResource.hasValue() ? this.metadataResource.value() : undefined;

    return {
      openGraphTitle: metadata?.openGraphTitle ?? "",
      openGraphDescription: metadata?.openGraphDescription ?? "",
      openGraphImageUrl: metadata?.openGraphImageUrl ?? "",
      canonicalUrl: metadata?.canonicalUrl ?? "",
      schemaOrgJson: metadata?.schemaOrgJson ? JSON.stringify(metadata.schemaOrgJson) : "{}" ,
      robot: metadata?.robot ?? "index,follow",
    }
  })
  protected readonly hasImage = linkedSignal(() => {
    const formSignal = this.formSignal();
    return formSignal.openGraphImageUrl !== undefined && formSignal.openGraphImageUrl !== null && formSignal.openGraphImageUrl !== "";
  })
  protected readonly isSubmitting = signal(false);

  protected readonly formModel = form(this.formSignal, (tree) => {
    disabled(tree, ({state}) => this.metadataResource.isLoading() || this.isSubmitting())
    readonly(tree.openGraphImageUrl);
    readonly(tree.canonicalUrl);
    validate(tree.schemaOrgJson, (value) => {
      try {
        JSON.parse(value.value());
        return null;
      } catch (error) {
        return {
          kind: 'json',
          message: 'Invalid JSON',
        }
      }
    })
  })
  private readonly metadataResource = rxResource({
    params: () => ({
      postId: this.post()?.id!
    }),
    stream: ({ params}) => {
      return this.postService.getPostMeta(params.postId)
    }
  })

  protected resetImage() {
    this.formSignal.update((form) => ({ ...form, openGraphImageUrl: "" }));
  }

  protected async onOgImageSelected($event: Event) {
    const file = ($event.target as HTMLInputElement).files?.[0];
    if (!file) {
      return;
    }

    const loading = this.toastService.loading('Uploading image...');
    try{
      const uploadResult = await this.editorService.uploadFile(this.post()?.id!, file);
      if(uploadResult.success)
      {
        this.formSignal.update((form) => ({ ...form, openGraphImageUrl: uploadResult.file.url }));
        loading.updateToast({
          type: 'success',
          duration: 5000,
        })
        loading.updateMessage("Image uploaded successfully");
        return;
      }


      loading.updateToast({
        type: 'error',
        duration: 5000,
      })
      loading.updateMessage("Failed to upload image");
    }catch (e) {
      loading.updateToast({
        type: 'error',
        duration: 5000,
      })
      loading.updateMessage("An unknown error occured while uploading image");
    }
  }

  protected updateMetadata() {
    if(this.formModel().invalid()){
      return;
    }

    if(this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.postService.updatePostMeta(this.post()?.id!, this.formSignal())
      .pipe(
        this.toastService.observe({
          success: 'Metadata updated successfully',
          error: 'Failed to update metadata',
          loading: 'Hold on...Updating metadata',
        })
      )
      .subscribe({
        next: () => {
          this.metadataResource.reload();
        },
        error: (err) => {
          console.error('Error updating metadata:', err);
        },
        complete: () => {
          this.isSubmitting.set(false);
        }
      })
  }
}
