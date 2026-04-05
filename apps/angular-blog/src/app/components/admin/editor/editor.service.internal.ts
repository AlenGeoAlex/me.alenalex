import {Injectable, model} from '@angular/core';
import {OutputData} from '@editorjs/editorjs';
import {BloggiBackendApiWebFeaturesPostEndpointsPreviewRenderRenderResponse} from '@services/api/generated-sdk';
import {BehaviorSubject} from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class EditorServiceInternal {

  private readonly _editorData = new BehaviorSubject<OutputData | undefined>(undefined);
  private readonly _editorPreview = new BehaviorSubject<BloggiBackendApiWebFeaturesPostEndpointsPreviewRenderRenderResponse | undefined>(undefined);


}
