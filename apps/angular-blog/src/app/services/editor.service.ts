import {inject, Injectable} from '@angular/core';
import EditorJS, {API, BlockMutationEvent, OutputData} from '@editorjs/editorjs';
import Header from '@editorjs/header';
// @ts-ignore
import LinkTool from '@editorjs/link';
import ImageTool from '@editorjs/image';
import {HashService} from '@services/hash.service';
import {PostService} from '@services/api/generated-sdk';
import {catchError, lastValueFrom, throwIfEmpty} from 'rxjs';
import {HotToastService} from '@ngxpert/hot-toast';
import {asProblemDetailsAsync} from '@utils/http-utils';
// @ts-ignore
import Table from '@editorjs/table'
// @ts-ignore
import Embed from '@editorjs/embed';
import EditorjsList from '@editorjs/list';
import Quote from '@editorjs/quote';
import CodeTool from '@editorjs/code';
import InlineCode from '@editorjs/inline-code';
import Paragraph from '@editorjs/paragraph';
import Warning from '@editorjs/warning';

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

  /**
   * Uploads a file associated with a specific post to the server and handles the file storage process.
   *
   * @param {string} postId - The unique identifier for the post the file is associated with.
   * @param {File} file - The file to be uploaded, containing metadata such as name, type, and size.
   * @return {Promise<ImageUploadResult>} A promise that resolves to an object indicating the upload success status and file URL.
   */
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

  /**
   * Uploads an image by its URL and associates it with a specific post.
   *
   * This method fetches the image from the provided URL and uploads it to the storage.
   * If the direct fetch fails, it falls back to saving the URL via the post service.
   *
   * @param {string} postId - The unique identifier of the post to associate the image with.
   * @param {string} url - The URL of the image to be uploaded.
   * @return {Promise<ImageUploadResult>} A promise that resolves to the result of the upload operation, including the uploaded image's URL and the original URL.
   */
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


  /**
   * Creates and initializes an instance of EditorJS with provided configuration.
   *
   * @param {string} postId - The unique identifier for the post, used to associate uploaded files or URLs.
   * @param {Object} [options] - Optional parameters for configuring the EditorJS instance.
   * @param {string} [options.holder] - The ID of the HTML container where the editor will be rendered. Defaults to 'editor-js'.
   * @param {OutputData} [options.data] - Preloaded data for initializing the editor's content.
   * @param {boolean} [options.setReadOnly] - Whether the editor should be in read-only mode. Defaults to false.
   * @param {(api: API, event: BlockMutationEvent | BlockMutationEvent[]) => void} [options.onChange] - Callback triggered when the editor content changes.
   * @return {EditorJS} The initialized instance of EditorJS.
   */
  public createEngine(postId: string, options?: {
    holder?: string,
    data?: OutputData,
    setReadOnly?: boolean,
    onChange?: (api: API, event: BlockMutationEvent | BlockMutationEvent[]) => void,
  }): EditorJS {
    return new EditorJS({
      holder: options?.holder ?? 'editor-js',
      readOnly: options?.setReadOnly ?? false,
      onChange: options?.onChange,
      tools: {
        header: Header,
        link: {
          class: LinkTool,
          config: {
            endpoint: `api/v1/post/${postId}/editor/crawl-meta`
          }
        },
        embed: Embed,
        quote: Quote,
        code: CodeTool,
        table: Table,
        warning: Warning,
        inlineCode: {
          class: InlineCode,
          shortcut: 'CMD+SHIFT+M',
        },
        list: {
          class: EditorjsList,
          inlineToolbar: true,
          config: {
            defaultStyle: 'unordered',
          }
        },
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
