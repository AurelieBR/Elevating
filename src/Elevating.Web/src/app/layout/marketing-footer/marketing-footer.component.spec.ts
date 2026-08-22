import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { MarketingFooter } from './marketing-footer.component';

describe('MarketingFooter', () => {
  it('contains only the available public and account destinations', async () => {
    await TestBed.configureTestingModule({
      imports: [MarketingFooter],
      providers: [provideRouter([])],
    }).compileComponents();

    const fixture = TestBed.createComponent(MarketingFooter);
    fixture.detectChanges();

    const hrefs = Array.from(
      fixture.nativeElement.querySelectorAll('nav a') as NodeListOf<HTMLAnchorElement>,
      (link) => link.getAttribute('href'),
    );
    const logo = fixture.nativeElement.querySelector('img') as HTMLImageElement;

    expect(hrefs).toEqual(['/', '/features', '/about', '/login', '/register']);
    expect(logo.getAttribute('src')).toBe('/brand/elevating-stepped-logo.png');
    expect(logo.getAttribute('width')).toBe('48');
    expect(fixture.nativeElement.textContent).not.toContain('Pricing');
  });
});
