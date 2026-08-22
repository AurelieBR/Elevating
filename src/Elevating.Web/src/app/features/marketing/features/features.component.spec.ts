import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AuthService } from '../../../core/auth/auth.service';
import { Features } from './features.component';

describe('Features', () => {
  it('renders only the truthful feature set and supplied imagery', async () => {
    await TestBed.configureTestingModule({
      imports: [Features],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            isAuthenticated: signal(false).asReadonly(),
          },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(Features);
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    const text = element.textContent ?? '';

    for (const heading of [
      'Goal planning',
      'Action breakdown',
      'Automatic progress',
      'Find what matters',
      'Deadlines in view',
      'Your goals stay yours',
    ]) {
      expect(text).toContain(heading);
    }

    for (const unsupportedClaim of ['Pricing', 'reminders', 'journaling', 'AI planning']) {
      expect(text).not.toContain(unsupportedClaim);
    }

    expect(element.querySelector('img[alt*="Laptop"]')?.getAttribute('src')).toBe(
      '/images/marketing/features-workspace.webp',
    );
    expect(element.querySelector('img[alt*="Monday goals"]')?.getAttribute('src')).toBe(
      '/images/marketing/monday-goals.webp',
    );
  });
});
