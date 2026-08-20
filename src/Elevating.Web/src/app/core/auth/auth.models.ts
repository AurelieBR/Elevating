export type AuthStatus = 'checking' | 'authenticated' | 'anonymous';

export interface AuthenticatedUser {
  userId: string;
  email: string;
}

export interface AuthenticationResponse extends AuthenticatedUser {
  accessToken: string;
  expiresAtUtc: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
}
