import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AuthService } from '../../../core/auth/auth.service';
import { About } from './about.component';

describe('About', () => {
  const authenticated = signal(false);
  let fixture: ComponentFixture<About>;

  beforeEach(async () => {
    authenticated.set(false);

    await TestBed.configureTestingModule({
      imports: [About],
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

    fixture = TestBed.createComponent(About);
    fixture.detectChanges();
  });

  it('presents Elevating as a factual full-stack portfolio project', () => {
    const element = fixture.nativeElement as HTMLElement;
    const text = element.textContent ?? '';

    expect(text).toContain('ABOUT THE PROJECT');
    expect(element.querySelector('h1')?.textContent).toContain(
      'Built to turn an idea into a real product.',
    );
    expect(text).toContain('independent full-stack portfolio project');
    expect(text).toContain('Built across the stack.');
    expect(text).toContain('Angular 21');
    expect(text).toContain('ASP.NET Core 10');
    expect(text).toContain('Rotating refresh sessions');
    expect(text).toContain('Azure Container Apps');
  });

  it('covers real application boundaries, security, quality checks, and deployment', () => {
    const text = fixture.nativeElement.textContent as string;

    for (const capability of [
      'Multi-user authentication',
      'Ownership & authorization',
      'Useful goal workflows',
      'Real querying',
      'Automated quality checks',
      'Cloud deployment',
    ]) {
      expect(text).toContain(capability);
    }

    expect(text).toContain('JWT access tokens');
    expect(text).toContain('authenticated user ownership');
    expect(text).toContain("project's CI workflow");
    expect(text).toContain('deployed across Azure services');
  });

  it('uses the project-process image and credits the builder with safe portfolio links', () => {
    const element = fixture.nativeElement as HTMLElement;
    const image = element.querySelector('img[alt*="writing a plan"]') as HTMLImageElement;
    const portfolioLinks = Array.from(
      element.querySelectorAll('a[href="https://aureliebrelubrelu.com/"]'),
    ) as HTMLAnchorElement[];

    expect(image.getAttribute('src')).toBe('/images/marketing/planning-workbook.webp');
    expect(element.textContent).toContain('Built by Aurélie Brelu Brelu.');
    expect(element.textContent).toContain('full-stack developer near Montréal');
    expect(portfolioLinks).toHaveLength(2);

    for (const link of portfolioLinks) {
      expect(link.getAttribute('target')).toBe('_blank');
      expect(link.getAttribute('rel')).toBe('noopener noreferrer');
    }
  });

  it('removes the old repeated philosophy and fictional company signals', () => {
    const text = fixture.nativeElement.textContent as string;

    for (const removedCopy of [
      'Thoughtful planning for real life',
      'A calmer relationship with progress',
      'Structure without the pressure',
      'Our principles',
      'customers served',
      'team members',
    ]) {
      expect(text).not.toContain(removedCopy);
    }
  });

  it('offers the real product journey and switches it for authenticated users', () => {
    let primaryCta = fixture.nativeElement.querySelector(
      'a[href="/register"]',
    ) as HTMLAnchorElement;

    expect(primaryCta.textContent).toContain('Explore Elevating');

    authenticated.set(true);
    fixture.detectChanges();
    primaryCta = fixture.nativeElement.querySelector('a[href="/goals"]') as HTMLAnchorElement;

    expect(primaryCta.textContent).toContain('Go to your goals');
    expect(fixture.nativeElement.querySelector('a[href="/register"]')).toBeNull();
  });
});
