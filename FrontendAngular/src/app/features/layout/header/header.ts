import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog'; 
import { ConfirmationService } from 'primeng/api';
import { BackendConfigService } from '../../../core/services/backend-config';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, ButtonModule, RouterLink, ConfirmDialogModule],
  providers: [ConfirmationService], 
  templateUrl: './header.html'
})
export class HeaderComponent {
  readonly backendService = inject(BackendConfigService);
  readonly authService = inject(AuthService);

  private readonly confirmationService = inject(ConfirmationService);
  private readonly router = inject(Router);

  handleSwitchBackend() {
    if (!this.authService.isLoggedIn()) {
      this.backendService.toggleBackend();
      return;
    }

    this.confirmationService.confirm({
      header: 'Change API Architecture?',
      message: 'Switching the backend will log you out because the two architectures use completely separate databases. Are you sure you want to proceed?',
      icon: 'pi pi-exclamation-triangle',
      acceptIcon: 'none',
      rejectIcon: 'none',
      rejectButtonStyleClass: 'p-button-text text-500',
      acceptButtonStyleClass: 'bg-logo-green border-none',
      accept: () => {
        this.authService.logout();
        this.backendService.toggleBackend();
        this.router.navigate(['/login']);
      }
    });
  }
}