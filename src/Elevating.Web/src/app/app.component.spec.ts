import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AppComponent } from './app.component';
import { AuthService } from './core/auth/auth.service';

describe('AppComponent', () => {
  const status = signal<'checking' | 'authenticated' | 'anonymous'>('anonymous');
  const authenticated = signal(false);

  beforeEach(async () => {
    status.set('anonymous');
    authenticated.set(false);

    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            authStatus: status.asReadonly(),
            isAuthenticated: authenticated.asReadonly(),
          },
        },
      ],
    }).compileComponents();
  });

  it('creates the app', () => {
    const fixture = TestBed.createComponent(AppComponent);

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('shows a neutral loading state while authentication is checking', () => {
    status.set('checking');
    const fixture = TestBed.createComponent(AppComponent);

    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Restoring your session');
  });
});
