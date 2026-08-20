import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { AuthenticationResponse } from './auth.models';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  const authUrl = `${environment.apiBaseUrl}/auth`;
  const response = (token: string): AuthenticationResponse => ({
    userId: '8d435f3e-f2a5-44aa-a5d6-cf87ef9c229c',
    email: 'alice@example.com',
    accessToken: token,
    expiresAtUtc: '2026-08-12T16:00:00Z',
  });

  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    vi.restoreAllMocks();
  });

  it('starts in checking state before initialization resolves', () => {
    expect(service.authStatus()).toBe('checking');
    expect(service.isAuthenticated()).toBe(false);
    expect(service.accessToken()).toBeNull();
    expect(service.currentUser()).toBeNull();
  });

  it('authenticates after a successful startup refresh', () => {
    service.initializeSession().subscribe();

    const request = http.expectOne(`${authUrl}/refresh`);
    expect(request.request.withCredentials).toBe(true);
    request.flush(response('startup-token'));

    expect(service.authStatus()).toBe('authenticated');
    expect(service.accessToken()).toBe('startup-token');
    expect(service.currentUser()).toEqual({
      userId: response('ignored').userId,
      email: 'alice@example.com',
    });
  });

  it('becomes anonymous when startup refresh is unauthorized', () => {
    service.initializeSession().subscribe();

    http.expectOne(`${authUrl}/refresh`).flush(null, {
      status: 401,
      statusText: 'Unauthorized',
    });

    expect(service.authStatus()).toBe('anonymous');
    expect(service.accessToken()).toBeNull();
    expect(service.currentUser()).toBeNull();
  });

  it('stores login state only in memory after successful login', () => {
    const localStorageSpy = vi.spyOn(Storage.prototype, 'setItem');
    const sessionStorageSpy = vi.spyOn(window.sessionStorage, 'setItem');

    service.login({ email: 'alice@example.com', password: 'StrongPass1' }).subscribe();

    const request = http.expectOne(`${authUrl}/login`);
    expect(request.request.withCredentials).toBe(true);
    request.flush(response('login-token'));

    expect(service.accessToken()).toBe('login-token');
    expect(service.currentUser()?.email).toBe('alice@example.com');
    expect(service.isAuthenticated()).toBe(true);
    expect(localStorageSpy).not.toHaveBeenCalled();
    expect(sessionStorageSpy).not.toHaveBeenCalled();
  });

  it('does not authenticate when login fails', () => {
    let receivedError = false;

    service.login({ email: 'alice@example.com', password: 'WrongPass1' }).subscribe({
      error: () => (receivedError = true),
    });

    http.expectOne(`${authUrl}/login`).flush(null, {
      status: 401,
      statusText: 'Unauthorized',
    });

    expect(receivedError).toBe(true);
    expect(service.isAuthenticated()).toBe(false);
    expect(service.accessToken()).toBeNull();
  });

  it('authenticates after registration', () => {
    service.register({ email: 'alice@example.com', password: 'StrongPass1' }).subscribe();

    const request = http.expectOne(`${authUrl}/register`);
    expect(request.request.withCredentials).toBe(true);
    request.flush(response('registration-token'));

    expect(service.isAuthenticated()).toBe(true);
    expect(service.accessToken()).toBe('registration-token');
  });

  it('replaces the access token after refresh', () => {
    service.login({ email: 'alice@example.com', password: 'StrongPass1' }).subscribe();
    http.expectOne(`${authUrl}/login`).flush(response('old-token'));

    service.refresh().subscribe();
    http.expectOne(`${authUrl}/refresh`).flush(response('fresh-token'));

    expect(service.accessToken()).toBe('fresh-token');
  });

  it('clears state after successful logout', () => {
    service.login({ email: 'alice@example.com', password: 'StrongPass1' }).subscribe();
    http.expectOne(`${authUrl}/login`).flush(response('token'));

    service.logout().subscribe();
    const request = http.expectOne(`${authUrl}/logout`);
    expect(request.request.withCredentials).toBe(true);
    request.flush(null);

    expect(service.authStatus()).toBe('anonymous');
    expect(service.accessToken()).toBeNull();
    expect(service.currentUser()).toBeNull();
  });

  it('clears state even when logout fails', () => {
    service.login({ email: 'alice@example.com', password: 'StrongPass1' }).subscribe();
    http.expectOne(`${authUrl}/login`).flush(response('token'));

    service.logout().subscribe();
    http.expectOne(`${authUrl}/logout`).flush(null, {
      status: 503,
      statusText: 'Unavailable',
    });

    expect(service.authStatus()).toBe('anonymous');
    expect(service.accessToken()).toBeNull();
    expect(service.currentUser()).toBeNull();
  });
});
