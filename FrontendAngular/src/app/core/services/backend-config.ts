import { Injectable, signal, computed } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class BackendConfigService {
  private readonly CLEAN_ARCH_URL = 'https://localhost:6001';
  private readonly VERTICAL_SLICE_URL = 'https://localhost:5001';

  private selectedUrl = signal<string>(
    localStorage.getItem('preferredBackend') || this.CLEAN_ARCH_URL
  );

  currentUrl = computed(() => this.selectedUrl());
  isCleanArchitecture = computed(() => this.selectedUrl() === this.CLEAN_ARCH_URL);

  toggleBackend() {
    const newUrl = this.isCleanArchitecture() 
      ? this.VERTICAL_SLICE_URL 
      : this.CLEAN_ARCH_URL;
    
    this.selectedUrl.set(newUrl);
    localStorage.setItem('preferredBackend', newUrl);
  }
}