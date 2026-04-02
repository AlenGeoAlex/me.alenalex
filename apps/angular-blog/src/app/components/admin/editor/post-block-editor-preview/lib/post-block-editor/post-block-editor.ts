import {Component, effect, inject, input, model, OnDestroy, output, signal, viewChild} from '@angular/core';
import {CommonModule} from '@angular/common';
import EditorJS, {OutputData} from '@editorjs/editorjs';
import {EditorService} from '@services/editor.service';
import {BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse} from '@services/api/generated-sdk';

@Component({
  selector: 'bloggi-post-block-editor',
  imports: [CommonModule],
  templateUrl: './post-block-editor.html',
  styleUrl: './post-block-editor.scss',
})
export class PostBlockEditor implements OnDestroy{

  public readonly post = input.required<BloggiBackendApiWebFeaturesPostEndpointsPostGetPostGetPostResponse | undefined>();
  protected readonly editorInitialized = signal(false);
  private _editor?: EditorJS ;
  private readonly editorService = inject(EditorService);
  public readonly editorLastMutatedOn = output<Date | undefined>();
  public readonly editorReady = output<boolean>();
  public readonly editorDirty = signal(false);
  public readonly isEditorDirty = output<boolean>();
  public readonly editorData = model<OutputData | undefined>();

  constructor() {
    effect(() => {
      const editorInitialized = this.editorInitialized();
      const post = this.post();
      const data = this.editorData();
      if(editorInitialized) return;

      this.destroyEditor();
      if(!post || !data)
      {
        this.setEditorReady(false);
        return;
      }

      this._editor = this.editorService.createEngine(
        post?.id!,
        {
          setReadOnly: post.status === 'Published',
          onChange: () => this.setEditorDirty(),
          data: data,
        });
      this.setEditorReady();
    })
    effect(() => {
      const dirty = this.editorDirty();
      this.isEditorDirty.emit(dirty);
    })
  }

  private setEditorDirty(){
    this.editorLastMutatedOn.emit(new Date());
    this._editor?.save()
      .then(x => {
        this.editorData.set(x);
      })
  }

  private setEditorReady(ready = true){
    if(ready){
      this.editorReady.emit(true);
      this.editorInitialized.set(true);
    } else{
      this.editorReady.emit(false);
      this.editorInitialized.set(false);
    }
  }

  private destroyEditor()
  {
    if(!this._editor)
      return;

    this._editor = undefined;
  }

  ngOnDestroy(): void {
    this.destroyEditor();
  }

  public get editor() {
    return this._editor!;
  }
}
