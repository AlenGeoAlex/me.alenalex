import {Component, computed, inject, linkedSignal, signal} from '@angular/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import {InputTextModule} from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import {form, FormField, required, email, max, maxLength, minLength} from '@angular/forms/signals';
import {MultiSelectFilterEvent, MultiSelectModule} from 'primeng/multiselect';
import {rxResource} from '@angular/core/rxjs-interop';
import {delay, of} from 'rxjs';
import {SelectFilterEvent, SelectModule } from 'primeng/select';
import {ButtonModule} from 'primeng/button';
import {DividerModule} from 'primeng/divider';
import {DynamicDialogRef} from 'primeng/dynamicdialog';
import {ProgressSpinnerModule} from 'primeng/progressspinner';

@Component({
  selector: 'bloggi-create-post',
  imports: [
    FloatLabelModule,
    InputTextModule,
    CardModule,
    FormField,
    MultiSelectModule,
    SelectModule,
    ButtonModule,
    DividerModule,
    ProgressSpinnerModule
  ],
  providers: [

  ],
  templateUrl: './create-post.html',
  styleUrl: './create-post.scss',
})
export class CreatePost {

  private readonly dialogRef = inject(DynamicDialogRef);
  protected readonly postModel = signal<PostFormModel>({
    title: '',
    excerpt: '',
    tags: [],
    seriesId: null,
  })
  protected readonly isSubmitting = signal(false);
  protected readonly postForm = form(this.postModel, (schemaPath) => {
    required(schemaPath.title);
    maxLength(schemaPath.title, 200);
    minLength(schemaPath.title, 5);

    maxLength(schemaPath.excerpt, 200);
    maxLength(schemaPath.tags, 10);
  })
  protected readonly isInvalid = linkedSignal(() => {
    const postForm = this.postForm();
    const invalid = postForm.invalid();
    const isSubmitting = this.isSubmitting();
    return invalid || isSubmitting;
  });
  protected readonly tagsSearch = signal('');
  protected readonly seriesSearch = signal('');
  protected readonly tags = rxResource({
    params: () => ({
      search: this.tagsSearch()
    }),
    stream: (params) => {
      return of([{
        label: 'Angular',
        id: 'angular'
      }]).pipe(
        delay(2000)
      )
    }
  })

  protected readonly series = rxResource({
    params: () => ({
      search: this.seriesSearch()
    }),
    stream: () => {
      return of([{
        label: 'Angular',
        id: 'angular'
      }]).pipe(
        delay(2000)
      )
    }
  })

  onSearchTags(event: MultiSelectFilterEvent) {
    this.tagsSearch.set(event.filter);
  }

  protected onSearchSeries($event: SelectFilterEvent) {
    this.seriesSearch.set($event.filter);
  }

  protected onReset() {
    this.postForm().reset({
      title: '',
      excerpt: '',
      tags: [],
      seriesId: null,
    })
  }

  protected onCreate() {
    this.isSubmitting.set(true);
    setTimeout(() => {
      this.isSubmitting.set(false);
    }, 2000)
  }
}

interface PostFormModel {
  title: string;
  excerpt: string;
  tags: string[];
  seriesId: string | null;
}
