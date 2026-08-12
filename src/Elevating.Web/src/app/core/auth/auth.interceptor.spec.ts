import {
  HttpClient,
  HttpErrorResponse,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';

import { environment } from '../../../environments/environment';
import { AuthenticationResponse } from './auth.models';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  const apiBaseUrl = environment.apiBaseUrl;
  const authUrl = `${apiBaseUrl}/auth`;
  const authentication = (accessToken: string): AuthenticationResponse => ({
    userId: '8d435f3e-f2a5-44aa-a5d6-cf87ef9c229c',
    email: 'alice@example.com',
    accessToken,
    expiresAtUtc: '2026-08-12T16:00:00Z',
  });

  let client: HttpClient;
  let http: HttpTestingController;
  let auth: AuthService;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    client = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    http.verify();
    vi.restoreAllMocks();
  });

  it('adds the bearer token to Elevating API requests when authenticated', () => {
    authenticate('access-token');

    client.get(`${apiBaseUrl}/goals`).subscribe();

    const request = http.expectOne(`${apiBaseUrl}/goals`);
    expect(request.request.headers.get('Authorization')).toBe('Bearer access-token');
    request.flush({});
  });

  it('does not add Authorization when anonymous', () => {
    auth.clearSession();

    client.get(`${apiBaseUrl}/goals`).subscribe();

    const request = http.expectOne(`${apiBaseUrl}/goals`);
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });

  it('does not attach the bearer token to unrelated URLs', () => {
    authenticate('access-token');

    client.get('https://example.org/data').subscribe();

    const request = http.expectOne('https://example.org/data');
    expect(request.request.headers.has('Authorization')).toBe(false);
    expect(request.request.withCredentials).toBe(false);
    request.flush({});
  });

  it('credentials refresh-cookie authentication requests', () => {
    client.post(`${authUrl}/refresh`, null).subscribe();

    const request = http.expectOne(`${authUrl}/refresh`);
    expect(request.request.withCredentials).toBe(true);
    request.flush(authentication('fresh-token'));
  });

  it('refreshes once and retries a protected request with the fresh token', () => {
    authenticate('expired-token');

    let completed = false;
    client.get(`${apiBaseUrl}/goals`).subscribe(() => (completed = true));

    const original = http.expectOne(`${apiBaseUrl}/goals`);
    expect(original.request.headers.get('Authorization')).toBe('Bearer expired-token');
    original.flush(null, { status: 401, statusText: 'Unauthorized' });

    http.expectOne(`${authUrl}/refresh`).flush(authentication('fresh-token'));

    const retry = http.expectOne(`${apiBaseUrl}/goals`);
    expect(retry.request.headers.get('Authorization')).toBe('Bearer fresh-token');
    retry.flush({});

    expect(completed).toBe(true);
  });

  it('clears authentication when refresh fails', () => {
    authenticate('expired-token');
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    let receivedStatus: number | undefined;
    client.get(`${apiBaseUrl}/goals`).subscribe({
      error: (error: HttpErrorResponse) => (receivedStatus = error.status),
    });

    http.expectOne(`${apiBaseUrl}/goals`).flush(null, {
      status: 401,
      statusText: 'Unauthorized',
    });
    http.expectOne(`${authUrl}/refresh`).flush(null, {
      status: 401,
      statusText: 'Unauthorized',
    });

    expect(receivedStatus).toBe(401);
    expect(auth.authStatus()).toBe('anonymous');
    expect(auth.accessToken()).toBeNull();
    expect(router.navigate).toHaveBeenCalledOnce();
  });

  it('preserves authentication when the retried request returns 404', () => {
    authenticate('expired-token');
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    let receivedStatus: number | undefined;
    client.get(`${apiBaseUrl}/goals/404`).subscribe({
      error: (error: HttpErrorResponse) => (receivedStatus = error.status),
    });

    http.expectOne(`${apiBaseUrl}/goals/404`).flush(null, {
      status: 401,
      statusText: 'Unauthorized',
    });
    http.expectOne(`${authUrl}/refresh`).flush(authentication('fresh-token'));
    http.expectOne(`${apiBaseUrl}/goals/404`).flush(null, {
      status: 404,
      statusText: 'Not Found',
    });

    expect(receivedStatus).toBe(404);
    expect(auth.authStatus()).toBe('authenticated');
    expect(auth.accessToken()).toBe('fresh-token');
    expect(router.navigate).not.toHaveBeenCalled();
  });
  it('does not recursively refresh a failed refresh request', () => {
    let receivedError = false;
    client.post(`${authUrl}/refresh`, null).subscribe({
      error: () => (receivedError = true),
    });

    http.expectOne(`${authUrl}/refresh`).flush(null, {
      status: 401,
      statusText: 'Unauthorized',
    });

    expect(receivedError).toBe(true);
    http.expectNone(`${authUrl}/refresh`);
  });

  it.each(['login', 'register'])('does not refresh after a failed %s request', (endpoint) => {
    let receivedError = false;
    client.post(`${authUrl}/${endpoint}`, {}).subscribe({
      error: () => (receivedError = true),
    });

    http.expectOne(`${authUrl}/${endpoint}`).flush(null, {
      status: 401,
      statusText: 'Unauthorized',
    });

    expect(receivedError).toBe(true);
    http.expectNone(`${authUrl}/refresh`);
  });

  it('shares one refresh across concurrent protected 401 responses', () => {
    authenticate('expired-token');

    client.get(`${apiBaseUrl}/goals/1`).subscribe();
    client.get(`${apiBaseUrl}/goals/2`).subscribe();

    http.expectOne(`${apiBaseUrl}/goals/1`).flush(null, {
      status: 401,
      statusText: 'Unauthorized',
    });
    http.expectOne(`${apiBaseUrl}/goals/2`).flush(null, {
      status: 401,
      statusText: 'Unauthorized',
    });

    const refreshRequests = http.match(`${authUrl}/refresh`);
    expect(refreshRequests).toHaveLength(1);
    refreshRequests[0].flush(authentication('fresh-token'));

    const retries = http.match(
      (request) =>
        request.url === `${apiBaseUrl}/goals/1` || request.url === `${apiBaseUrl}/goals/2`,
    );
    expect(retries).toHaveLength(2);
    expect(
      retries.every(
        (request) => request.request.headers.get('Authorization') === 'Bearer fresh-token',
      ),
    ).toBe(true);
    retries.forEach((request) => request.flush({}));
  });

  function authenticate(accessToken: string): void {
    auth.login({ email: 'alice@example.com', password: 'StrongPass1' }).subscribe();
    http.expectOne(`${authUrl}/login`).flush(authentication(accessToken));
  }
});
