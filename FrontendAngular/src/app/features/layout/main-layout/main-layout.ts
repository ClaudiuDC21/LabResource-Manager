import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router } from '@angular/router';
import { HeaderComponent } from '../header/header';
import { FooterComponent } from '../footer/footer';
import { SidebarComponent } from '../sidebar/sidebar';
import { LayoutService } from '../services/layout.service';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, HeaderComponent, FooterComponent, SidebarComponent],
  templateUrl: './main-layout.html'
})
export class MainLayoutComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  readonly layoutService = inject(LayoutService); // Am injectat serviciul

  get showSidebar(): boolean {
    return this.authService.isLoggedIn() && this.router.url !== '/';
  }
}