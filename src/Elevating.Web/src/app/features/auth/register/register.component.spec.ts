import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import { Register } from './register.component';

describe('Register', () => {
  const user = {
    userId: '8d435f3e-f2a5-44aa-a5d6-cf87ef9c229c',
    email: 'alice@example.com',
  };

  const auth = {
    register: vi.fn(),
  };

  let component: Register;
  let fixture: ComponentFixture<Register>;
  let router: Router;

  beforeEach(async () => {
    auth.register.mockReset();

    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [provideRouter([]), { provide: AuthService, useValue: auth }],
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
  });

  it('submits valid registration and navigates to the application', () => {
    auth.register.mockReturnValue(of(user));
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    component.form.setValue({
      email: 'alice@example.com',
      password: 'StrongPass1',
      passwordConfirmation: 'StrongPass1',
    });

    component.submit();

    expect(auth.register).toHaveBeenCalledWith({
      email: 'alice@example.com',
      password: 'StrongPass1',
    });
    expect(router.navigateByUrl).toHaveBeenCalledWith('/goals');
  });

  it('enforces the backend password policy for UX validation', () => {
    component.form.setValue({
      email: 'alice@example.com',
      password: 'alllowercase',
      passwordConfirmation: 'alllowercase',
    });

    expect(component.form.controls.password.invalid).toBe(true);

    component.form.controls.password.setValue('ALLUPPERCASE1');
    component.form.controls.passwordConfirmation.setValue('ALLUPPERCASE1');
    expect(component.form.controls.password.invalid).toBe(true);

    component.form.controls.password.setValue('NoDigitsHere');
    component.form.controls.passwordConfirmation.setValue('NoDigitsHere');
    expect(component.form.controls.password.invalid).toBe(true);
  });

  it('detects a password confirmation mismatch', () => {
    component.form.setValue({
      email: 'alice@example.com',
      password: 'StrongPass1',
      passwordConfirmation: 'DifferentPass1',
    });

    expect(component.form.hasError('passwordMismatch')).toBe(true);

    component.submit();
    expect(auth.register).not.toHaveBeenCalled();
  });

  it('handles a duplicate email safely', () => {
    auth.register.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            statusText: 'Conflict',
          }),
      ),
    );
    component.form.setValue({
      email: 'alice@example.com',
      password: 'StrongPass1',
      passwordConfirmation: 'StrongPass1',
    });

    component.submit();

    expect(component.errorMessage()).toBe('An account with this email already exists.');
  });
});
