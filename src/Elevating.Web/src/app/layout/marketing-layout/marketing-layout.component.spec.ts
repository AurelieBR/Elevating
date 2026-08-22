import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AuthService } from '../../core/auth/auth.service';
import { MarketingLayout } from './marketing-layout.component';

describe('MarketingLayout', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MarketingLayout],
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

  it('wraps public content with the marketing header and footer', () => {
    const fixture = TestBed.createComponent(MarketingLayout);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('app-marketing-header')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('router-outlet')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('app-marketing-footer')).not.toBeNull();
  });
});
