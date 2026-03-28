import {Component, inject, signal} from '@angular/core';
import {ToolbarModule} from 'primeng/toolbar';
import {ButtonModule} from 'primeng/button';
import { SelectButtonModule } from 'primeng/selectbutton';
import {FormsModule} from '@angular/forms';
import {ToggleSwitchModule} from 'primeng/toggleswitch';
import {DialogService, DynamicDialogRef, DynamicDialogModule} from 'primeng/dynamicdialog';
import {CreatePost} from '@components/admin/dashboard/create-post/create-post';

@Component({
  selector: 'bloggi-post-list',
  imports: [
    ToolbarModule,
    ButtonModule,
    SelectButtonModule,
    FormsModule,
    ToggleSwitchModule,
    DynamicDialogModule,
  ],
  providers: [DialogService],
  templateUrl: './post-list.html',
  styleUrl: './post-list.scss',
})
export class PostList {

  protected tableState = [{
    label: 'Post',
    value: 'post',
  }, {
    label: 'Series',
    value: 'series',
  }];
  protected readonly currentState = signal<string>(this.tableState[0].value);
  protected readonly tableSelectedIds = signal<string[]>([]);
  protected readonly dialogService = inject(DialogService);

  private readonly newPostDialogRef = signal<DynamicDialogRef | null>(null);

  protected openNew() {
    const dialogRef = this.dialogService.open(
      CreatePost,
      {
        modal: false,
        width: '50vw',
        height: '70vh',
        dismissableMask: true,
        baseZIndex: 10000,
        header: 'Create New Post',
      }
    )

    if(!dialogRef) return;

    dialogRef.onClose.subscribe(() => {
      this.newPostDialogRef.set(null);
    });

    this.newPostDialogRef.set(dialogRef)
  }

  protected draftSelected() {

  }


}
