import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import { Login } from './login.component';

describe('Login', () => {
  const user = {
    userId: '8d435f3e-f2a5-44aa-a5d6-cf87ef9c229c',
    email: 'alice@example.com',
  };

  const auth = {
    login: vi.fn(),
  };

  let component: Login;
  let fixture: ComponentFixture<Login>;
  let router: Router;

  beforeEach(async () => {
    auth.login.mockReset();

    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: auth },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: convertToParamMap({ returnUrl: '/goals/42' }),
            },
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
  });

  it('submits a valid form and navigates to the safe return URL', () => {
    auth.login.mockReturnValue(of(user));
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    component.form.setValue({
      email: 'alice@example.com',
      password: 'StrongPass1',
    });

    component.submit();

    expect(auth.login).toHaveBeenCalledWith({
      email: 'alice@example.com',
      password: 'StrongPass1',
    });
    expect(router.navigateByUrl).toHaveBeenCalledWith('/goals/42');
  });

  it('shows a generic invalid-credentials error', () => {
    auth.login.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 401,
            statusText: 'Unauthorized',
          }),
      ),
    );
    component.form.setValue({
      email: 'alice@example.com',
      password: 'WrongPass1',
    });

    component.submit();

    expect(component.errorMessage()).toBe('Invalid email or password.');
  });

  it('does not submit invalid or duplicate requests', () => {
    component.submit();
    expect(auth.login).not.toHaveBeenCalled();

    const pending = new Subject<typeof user>();
    auth.login.mockReturnValue(pending.asObservable());
    component.form.setValue({
      email: 'alice@example.com',
      password: 'StrongPass1',
    });

    component.submit();
    component.submit();

    expect(auth.login).toHaveBeenCalledOnce();
    pending.complete();
  });
});
