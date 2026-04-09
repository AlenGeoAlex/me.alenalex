import {Component, inject, input} from '@angular/core';
import {Button} from 'primeng/button';
import {InputText} from 'primeng/inputtext';
import {Select} from 'primeng/select';
import {FormsModule} from '@angular/forms';
import {
  BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse,
  PostService
} from '@services/api/generated-sdk';
import {HotToastService} from '@ngxpert/hot-toast';

@Component({
  selector: 'bloggi-post-metadata-editor',
  imports: [
    Button,
    InputText,
    Select,
    FormsModule
  ],
  templateUrl: './post-metadata-editor.html',
  styleUrl: './post-metadata-editor.scss',
})
export class PostMetadataEditor {

  public readonly post = input.required<BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse | undefined>();
  private readonly toastService = inject(HotToastService);
  private readonly postService = inject(PostService);

}
