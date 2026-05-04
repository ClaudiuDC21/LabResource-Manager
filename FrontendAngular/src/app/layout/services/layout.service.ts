import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LayoutService {
  isSidebarExpanded = signal<boolean>(true);

  toggleSidebar() {
    this.isSidebarExpanded.update(val => !val);
  }
}