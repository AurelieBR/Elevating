import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-marketing-header',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './marketing-header.component.html',
  styleUrl: './marketing-header.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MarketingHeader {
  readonly auth = inject(AuthService);
  readonly menuOpen = signal(false);

  toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }
}
