import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { BackendConfigService } from '../../../core/services/backend-config';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, ButtonModule, RouterLink],
  templateUrl: './header.html'
})
export class HeaderComponent {
  backendService = inject(BackendConfigService);

  isLoggedIn = false;
}