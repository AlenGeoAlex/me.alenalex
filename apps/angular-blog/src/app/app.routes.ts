import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home').then((m) => m.Home),
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
        //loadComponent: () => import('./pages/auth/google-oauth/google-oauth-callback').then((m) => m.GoogleOAuthCallback),
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
