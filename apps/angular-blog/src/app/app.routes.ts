import { Routes } from '@angular/router';
import {accessGuard} from '@guards/access-guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home').then((m) => m.Home),
  },
  {
    path: 'admin',
    canActivate: [accessGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/admin/admin-home/admin-home').then((m) => m.AdminHome),
      }
    ]
  },
  {
    path: 'auth',
    children: [
      {
        path: 'google',
        loadComponent: () => import('./pages/auth/google-oauth/google-oauth').then((m) => m.GoogleOauth),
      },
      {
        path: 'google/callback',
        loadComponent: () => import('./pages/auth/google-callback/google-callback').then((m) => m.GoogleCallback),
      }
    ]
  },
  {
    path: 'admin',
    children: [

    ]
  },
  {
    path: '',
    redirectTo: '/',
    pathMatch: 'full',
  },
];
