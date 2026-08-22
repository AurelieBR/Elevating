import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AuthService } from '../../../core/auth/auth.service';
import { Home } from './home.component';

describe('Home', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Home],
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
  });

  it('renders the promise, real hero asset, and anonymous journey links', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    const links = Array.from(element.querySelectorAll('a'));

    expect(element.querySelector('h1')?.textContent).toContain('Elevate your day, every day.');
    expect(
      links.find((link) => link.textContent?.trim() === 'Get started')?.getAttribute('href'),
    ).toBe('/register');
    expect(links.find((link) => link.textContent?.includes('Sign in'))?.getAttribute('href')).toBe(
      '/login',
    );
    expect(element.querySelector('img[alt*="Notebook"]')?.getAttribute('src')).toBe(
      '/images/marketing/home-hero.webp',
    );
  });

  it('uses a static, accessible product preview', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;

    expect(element.textContent).toContain("Today's focus");
    expect(element.textContent).toContain('A calmer morning routine');
    expect(element.querySelector('[role="progressbar"]')?.getAttribute('aria-valuenow')).toBe('60');
  });
});
