import {inject, Injectable} from '@angular/core';
import EditorJS, {OutputData} from '@editorjs/editorjs';
import Header from '@editorjs/header';
// @ts-ignore
import LinkTool from '@editorjs/link';
import ImageTool from '@editorjs/image';
import {HashService} from '@services/hash.service';
import {PostService} from '@services/api/generated-sdk';
import {catchError, lastValueFrom, throwIfEmpty} from 'rxjs';
import {HotToastService} from '@ngxpert/hot-toast';
import {asProblemDetailsAsync} from '@utils/http-utils';

type ImageUploadResult = {
  success: number
  file: {
    url: string
    publicUrl?: string
  }
}

@Injectable({
  providedIn: 'root',
})
export class EditorService {


  private readonly hashService = inject(HashService);
  private readonly postService = inject(PostService);
  private readonly toastService = inject(HotToastService);

  private async uploadFile(postId: string, file: File): Promise<ImageUploadResult> {
    const fileHash = await this.hashService.hash(file);

    const response = await lastValueFrom(this.postService.saveFile(postId, {
      name: file.name,
      hash: fileHash,
      contentType: file.type,
      size: file.size,
    }).pipe(
        catchError(err => {
          asProblemDetailsAsync(err)
            .then(problemDetails => {
              console.log(problemDetails)
              this.toastService.error(`An error occurred in the while uploading the image: ${problemDetails.detail}`)
            })

          throw err;
        }),
      )
    );

    if (!response.expiresAt) {
      return {
        success: 1,
        file: {
          url: response.url!
        }
      }
    }

    const uploadResponse = await fetch(response.signedUrl!, {
      method: 'PUT',
      headers: { 'Content-Type': file.type },
      body: file,
    });

    if (!uploadResponse.ok) {
      this.toastService.error(`Failed to upload image: ${uploadResponse.statusText}`);
      return {
        success: 0,
        file: {
          url: ''
        }
      }
    }

    return {
      success: 1,
      file: {
        url: response.url!
      }
    }
  }

  private async uploadByUrl(postId: string, url: string): Promise<ImageUploadResult> {
    try {
      const fetchResponse = await fetch(url);

      if (!fetchResponse.ok) {
        throw new Error(`Failed to fetch image from URL: ${fetchResponse.statusText}`);
      }

      const blob = await fetchResponse.blob();
      const filename = url.split('/').pop()?.split('?')[0] ?? 'image';
      const file = new File([blob], filename, { type: blob.type });

      const responseUrl = await this.uploadFile(postId, file);
      return {
        success: 1,
        file: {
          url: responseUrl.file.url,
          publicUrl: url
        }
      }
    } catch {
      const response = await lastValueFrom(
        this.postService.saveUrl(postId, { url })
          .pipe(
            catchError(err => {
              asProblemDetailsAsync(err)
                .then(problemDetails => {
                  console.log(problemDetails)
                  this.toastService.error(`An error occurred in the backend while fetching the image: ${problemDetails.detail}`)
                })

              throw err;
            }),
          )
      );
      return {
        success: 1,
        file: {
          url: response.url!,
          publicUrl: url
        }
      }
    }
  }

  createEngine(postId: string, options?: { holder?: string, data?: OutputData }): EditorJS {
    return new EditorJS({
      holder: options?.holder ?? 'editor-js',
      tools: {
        header: Header,
        link: LinkTool,
        image: {
          class: ImageTool,
          config: {
            uploader: {
              uploadByFile: (file: File) => this.uploadFile(postId, file),
              uploadByUrl: (url: string) => this.uploadByUrl(postId, url),
            }
          }
        }
      },
      data: options?.data,
    });
  }
}
