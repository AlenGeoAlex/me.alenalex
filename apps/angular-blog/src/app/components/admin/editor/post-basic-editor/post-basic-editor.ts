import {Component, effect, inject, input, signal} from '@angular/core';
import {
  BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse,
  BloggiBackendApiWebFeaturesPostEndpointsPostUpdatePostUpdatePostRequest, PostService
} from '@services/api/generated-sdk';
import {HotToastService} from '@ngxpert/hot-toast';
import {Textarea} from 'primeng/textarea';
import {form, FormField, maxLength, minLength, required} from '@angular/forms/signals';
import {AutoComplete} from 'primeng/autocomplete';
import {Button} from 'primeng/button';
import {InputText} from 'primeng/inputtext';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'bloggi-post-basic-editor',
  imports: [
    Textarea,
    AutoComplete,
    Button,
    InputText,
    FormField,
    FormsModule,
  ],
  templateUrl: './post-basic-editor.html',
  styleUrl: './post-basic-editor.scss',
})
export class PostBasicEditor {

  public readonly post = input.required<BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse | undefined>();
  private readonly toastService = inject(HotToastService);
  private readonly postService = inject(PostService);
  private readonly formSignal = signal({
    title: '',
    excerpt: '',
    tags: [] as string[],
  });
  protected readonly isSubmitting = signal(false);
  protected readonly tags = signal<string[]>([]);
  constructor() {
    effect(() => {
      const post = this.post();
      this.tags.set((post?.tags ?? []).map(x => x.displayName!));
      this.formSignal.set({
        title: post?.title || '',
        excerpt: post?.excerpt || '',
        tags: this.tags()
      })
    })
  }

  protected readonly formModel = form(this.formSignal, (schemaPath) => {
    required(schemaPath.title);
    maxLength(schemaPath.title, 200);
    minLength(schemaPath.title, 5);

    maxLength(schemaPath.excerpt!, 200);
  });

  protected save(){
    if(this.formModel().invalid()){
      return;
    }

    const payload: BloggiBackendApiWebFeaturesPostEndpointsPostUpdatePostUpdatePostRequest = {
      ...this.formSignal(),
      tags: this.tags(),
    };

    this.postService.updatePost(this.post()?.id!, this.formSignal())
      .pipe(
        this.toastService.observe({
          loading: 'Hold on...Saving post',
          error: 'Failed to save post',
          success: 'Post saved successfully',
        })
      ).subscribe()
  }
}
