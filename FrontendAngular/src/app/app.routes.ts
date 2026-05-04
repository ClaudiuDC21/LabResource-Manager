import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home';
import { MainLayoutComponent } from './layout/main-layout/main-layout';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { ProfileComponent } from './features/profile/profile';
import { DashboardComponent } from './features/dashboard/dashboard';
import { MyBorrowingsComponent } from './features/my-borrowings/my-borrowings';
import { UsersComponent } from './features/users/users';
import { authGuard } from './core/guards/auth.guard';
import { teacherGuard } from './core/guards/teacher.guard';
import { UserDetailsComponent } from './features/users/user-details/user-details';
import { AssetRequestComponent } from './features/borrowings/asset-request/asset-request';
import { ApprovalsComponent } from './features/approvals/approvals';
import { AssetDetailsComponent } from './features/asset-details/asset-details';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', component: HomeComponent },
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent },
      { 
        path: 'profile', 
        component: ProfileComponent,
        canActivate: [authGuard] 
      },
      { 
        path: 'dashboard', 
        component: DashboardComponent,
        canActivate: [authGuard] 
      },
      { 
        path: 'my-borrowings', 
        component: MyBorrowingsComponent,
        canActivate: [authGuard] 
      },
      { 
        path: 'users', 
        component: UsersComponent,
        canActivate: [authGuard, teacherGuard] 
      },
      { 
        path: 'users/:id', 
        component: UserDetailsComponent,
        canActivate: [authGuard, teacherGuard] 
      },
      { 
        path: 'dashboard/:id/request', 
        component: AssetRequestComponent,
        canActivate: [authGuard] 
      },
      {
        path: 'approvals',
        component: ApprovalsComponent,
        canActivate: [authGuard, teacherGuard]
      },
      {
        path: 'dashboard/asset/:id',
        component: AssetDetailsComponent,
        canActivate: [authGuard]
      }
    ]
  },
  { path: '**', redirectTo: '' }
];