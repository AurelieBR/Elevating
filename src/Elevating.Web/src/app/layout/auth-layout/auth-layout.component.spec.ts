import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AuthLayout } from './auth-layout.component';

describe('AuthLayout', () => {
  it('provides real Elevating branding, a visible Home route, and routed auth content', async () => {
    await TestBed.configureTestingModule({
      imports: [AuthLayout],
      providers: [provideRouter([])],
    }).compileComponents();

    const fixture = TestBed.createComponent(AuthLayout);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    const brand = element.querySelector('.auth-lockup') as HTMLAnchorElement;
    const homeLink = element.querySelector('.auth-home-link') as HTMLAnchorElement;

    expect(brand.getAttribute('href')).toBe('/');
    expect(brand.querySelector('img')?.getAttribute('src')).toBe(
      '/brand/elevating-stepped-logo.png',
    );
    expect(homeLink.getAttribute('href')).toBe('/');
    expect(homeLink.textContent).toContain('Back to home');
    expect(element.querySelector('router-outlet')).not.toBeNull();
    expect(element.querySelector('app-marketing-header')).toBeNull();
  });
});
