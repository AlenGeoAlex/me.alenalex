import { Component } from '@angular/core';
import {PostList} from '../../../components/admin/dashboard/post-list/post-list';
import {Metric} from '../../../components/admin/dashboard/metric/metric';

@Component({
  selector: 'bloggi-admin-home',
  imports: [
    PostList,
    Metric
  ],
  templateUrl: './admin-home.html',
  styleUrl: './admin-home.scss',
})
export class AdminHome {}
