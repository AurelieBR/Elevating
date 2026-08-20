import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import {
  catchError,
  filter,
  finalize,
  map,
  Observable,
  of,
  shareReplay,
  take,
  tap,
  throwError,
} from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  AuthenticatedUser,
  AuthenticationResponse,
  AuthStatus,
  LoginRequest,
  RegisterRequest,
} from './auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly authUrl = `${environment.apiBaseUrl}/auth`;

  private readonly accessTokenState = signal<string | null>(null);
  private readonly currentUserState = signal<AuthenticatedUser | null>(null);
  private readonly authStatusState = signal<AuthStatus>('checking');
  private readonly authStatusChanges = toObservable(this.authStatusState);

  private initializationRequest: Observable<void> | null = null;
  private refreshRequest: Observable<string> | null = null;

  readonly accessToken = this.accessTokenState.asReadonly();
  readonly currentUser = this.currentUserState.asReadonly();
  readonly authStatus = this.authStatusState.asReadonly();
  readonly isAuthenticated = computed(() => this.authStatusState() === 'authenticated');

  initializeSession(): Observable<void> {
    if (this.authStatusState() !== 'checking') {
      return of(undefined);
    }

    if (this.initializationRequest === null) {
      this.initializationRequest = this.refresh().pipe(
        map(() => undefined),
        catchError(() => of(undefined)),
        shareReplay({ bufferSize: 1, refCount: false }),
      );
    }

    return this.initializationRequest;
  }

  login(request: LoginRequest): Observable<AuthenticatedUser> {
    return this.authenticate(
      this.http.post<AuthenticationResponse>(`${this.authUrl}/login`, request, {
        withCredentials: true,
      }),
    );
  }

  register(request: RegisterRequest): Observable<AuthenticatedUser> {
    return this.authenticate(
      this.http.post<AuthenticationResponse>(`${this.authUrl}/register`, request, {
        withCredentials: true,
      }),
    );
  }

  refresh(): Observable<string> {
    if (this.refreshRequest !== null) {
      return this.refreshRequest;
    }

    const request = this.http
      .post<AuthenticationResponse>(`${this.authUrl}/refresh`, null, {
        withCredentials: true,
      })
      .pipe(
        tap((response) => this.applyAuthentication(response)),
        map((response) => response.accessToken),
        catchError((error: unknown) => {
          this.clearSession();
          return throwError(() => error);
        }),
        finalize(() => {
          if (this.refreshRequest === request) {
            this.refreshRequest = null;
          }
        }),
        shareReplay({ bufferSize: 1, refCount: false }),
      );

    this.refreshRequest = request;
    return request;
  }

  logout(): Observable<void> {
    return this.http
      .post<void>(`${this.authUrl}/logout`, null, {
        withCredentials: true,
      })
      .pipe(
        catchError(() => of(undefined)),
        finalize(() => this.clearSession()),
      );
  }

  clearSession(): void {
    this.accessTokenState.set(null);
    this.currentUserState.set(null);
    this.authStatusState.set('anonymous');
  }

  whenInitialized(): Observable<AuthStatus> {
    return this.authStatusChanges.pipe(
      // Startup owns the only checking -> resolved transition.
      // Filtering here keeps guards from redirecting prematurely.
      filter((status) => status !== 'checking'),
      take(1),
    );
  }

  private authenticate(request: Observable<AuthenticationResponse>): Observable<AuthenticatedUser> {
    return request.pipe(
      tap((response) => this.applyAuthentication(response)),
      map((response) => ({
        userId: response.userId,
        email: response.email,
      })),
    );
  }

  private applyAuthentication(response: AuthenticationResponse): void {
    this.accessTokenState.set(response.accessToken);
    this.currentUserState.set({
      userId: response.userId,
      email: response.email,
    });
    this.authStatusState.set('authenticated');
  }
}
