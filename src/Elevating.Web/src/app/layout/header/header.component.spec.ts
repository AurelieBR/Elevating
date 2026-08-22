import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { Header } from './header.component';

describe('Header', () => {
  const auth = {
    currentUser: signal({
      userId: '8d435f3e-f2a5-44aa-a5d6-cf87ef9c229c',
      email: 'alice@example.com',
    }).asReadonly(),
    logout: vi.fn(),
  };

  let component: Header;
  let fixture: ComponentFixture<Header>;
  let router: Router;

  beforeEach(async () => {
    auth.logout.mockReset();
    auth.logout.mockReturnValue(of(undefined));

    await TestBed.configureTestingModule({
      imports: [Header],
      providers: [provideRouter([]), { provide: AuthService, useValue: auth }],
    }).compileComponents();

    fixture = TestBed.createComponent(Header);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
  });

  it('shows the authenticated user', () => {
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('alice@example.com');
  });

  it('uses the real brand asset and links back to /goals', () => {
    fixture.detectChanges();

    const brand = fixture.nativeElement.querySelector('.app-brand') as HTMLAnchorElement;
    const logo = brand.querySelector('img') as HTMLImageElement;

    expect(brand.getAttribute('href')).toBe('/goals');
    expect(logo.getAttribute('src')).toBe('/brand/elevating-stepped-logo.png');
    expect(logo.getAttribute('width')).toBe('46');
  });

  it('logs out and navigates to login', () => {
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    component.logout();

    expect(auth.logout).toHaveBeenCalledOnce();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
