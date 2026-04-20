import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog'; // Pentru Popup
import { ConfirmationService } from 'primeng/api'; // Pentru a controla Popup-ul
import { BackendConfigService } from '../../../core/services/backend-config';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, ButtonModule, RouterLink, ConfirmDialogModule],
  providers: [ConfirmationService], // Trebuie adăugat aici ca să funcționeze!
  templateUrl: './header.html'
})
export class HeaderComponent {
  backendService = inject(BackendConfigService);
  authService = inject(AuthService);
  
  private confirmationService = inject(ConfirmationService);
  private router = inject(Router);

  // Funcția care înlocuiește schimbarea directă
  handleSwitchBackend() {
    // Dacă nu este logat, schimbă direct (cum făcea până acum)
    if (!this.authService.isLoggedIn()) {
      this.backendService.toggleBackend();
      return;
    }

    // Dacă ESTE logat, declanșăm popup-ul
    this.confirmationService.confirm({
      header: 'Change API Architecture?',
      message: 'Switching the backend will log you out because the two architectures use completely separate databases. Are you sure you want to proceed?',
      icon: 'pi pi-exclamation-triangle',
      acceptIcon: 'none',
      rejectIcon: 'none',
      rejectButtonStyleClass: 'p-button-text text-500',
      acceptButtonStyleClass: 'bg-logo-green border-none',
      accept: () => {
        // Abia AICI, dacă apasă "Yes", schimbăm efectiv API-ul
        this.authService.logout();
        this.backendService.toggleBackend();
        this.router.navigate(['/login']);
      }
    });
  }
}