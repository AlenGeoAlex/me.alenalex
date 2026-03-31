import { Injectable } from '@angular/core';
import EditorJS, {OutputData} from '@editorjs/editorjs';
import Header from '@editorjs/header';
// @ts-ignore
import LinkTool from '@editorjs/link';

@Injectable({
  providedIn: 'root',
})
export class EditorService {

  createEngine(
    postId: string,
    options? : {
      holder?: string,
      data?: OutputData,
    }
  ) : EditorJS {
    const holder = options?.holder ?? 'editor-js';
    return new EditorJS({
      holder: holder,
      tools: {
        header: Header,
        link: LinkTool,
      },
      data: options?.data,
    })

  }
}
