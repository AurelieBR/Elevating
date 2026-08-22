import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';

import { AuthService } from '../../core/auth/auth.service';
import { MarketingHeader } from './marketing-header.component';

describe('MarketingHeader', () => {
  const authenticated = signal(false);
  let fixture: ComponentFixture<MarketingHeader>;
  let router: Router;

  beforeEach(async () => {
    authenticated.set(false);

    await TestBed.configureTestingModule({
      imports: [MarketingHeader],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            isAuthenticated: authenticated.asReadonly(),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MarketingHeader);
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('shows real public routes and anonymous account actions', () => {
    const element = fixture.nativeElement as HTMLElement;
    const text = fixture.nativeElement.textContent as string;
    const logo = element.querySelector('.brand-lockup img') as HTMLImageElement;
    const desktopNavHrefs = Array.from(
      element.querySelectorAll('nav[aria-label="Primary navigation"] a'),
      (link) => link.getAttribute('href'),
    );

    expect(element.querySelector('header')).not.toBeNull();
    expect(logo.getAttribute('src')).toBe('/brand/elevating-stepped-logo.png');
    expect(logo.getAttribute('width')).toBe('56');
    expect(desktopNavHrefs).toEqual(['/features', '/about']);
    expect(text).toContain('Features');
    expect(text).toContain('About');
    expect(text).toContain('Sign in');
    expect(text).toContain('Get started');
    expect(text).not.toContain('Pricing');
  });

  it('replaces acquisition actions with a goals link when authenticated', () => {
    authenticated.set(true);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Your goals');
    expect(text).not.toContain('Sign in');
    expect(text).not.toContain('Get started');
  });

  it('updates accessible mobile menu state and closes after navigation', async () => {
    const button = fixture.nativeElement.querySelector(
      'button[aria-controls="marketing-mobile-menu"]',
    ) as HTMLButtonElement;

    expect(button.getAttribute('aria-expanded')).toBe('false');

    button.click();
    fixture.detectChanges();

    expect(button.getAttribute('aria-expanded')).toBe('true');
    expect(fixture.nativeElement.querySelector('#marketing-mobile-menu')).not.toBeNull();

    const featureLink = fixture.nativeElement.querySelector(
      '#marketing-mobile-menu a[href="/features"]',
    ) as HTMLAnchorElement;
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    featureLink.click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(button.getAttribute('aria-expanded')).toBe('false');
    expect(fixture.nativeElement.querySelector('#marketing-mobile-menu')).toBeNull();
  });
});
