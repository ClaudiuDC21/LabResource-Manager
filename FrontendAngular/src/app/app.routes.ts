import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home';
import { MainLayoutComponent } from './features/layout/main-layout/main-layout';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { ProfileComponent } from './features/profile/profile';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', component: HomeComponent },
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent },
      { path: 'profile', component: ProfileComponent },
      { 
        path: 'dashboard', 
        children: []
      },
    ]
  },
  { path: '**', redirectTo: '' }
];