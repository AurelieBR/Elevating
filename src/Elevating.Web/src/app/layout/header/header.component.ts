import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-header',
  imports: [RouterLink],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Header {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loggingOut = signal(false);

  logout(): void {
    if (this.loggingOut()) {
      return;
    }

    this.loggingOut.set(true);

    this.auth
      .logout()
      .pipe(finalize(() => this.loggingOut.set(false)))
      .subscribe(() => {
        void this.router.navigate(['/login']);
      });
  }
}
