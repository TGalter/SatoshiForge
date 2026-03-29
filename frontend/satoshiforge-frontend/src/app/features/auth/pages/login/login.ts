import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize, switchMap, tap } from 'rxjs';

import { AuthSessionService } from '../../services/auth-session.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly authSessionService = inject(AuthSessionService);
  private readonly router = inject(Router);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.authSessionService.login(this.form.getRawValue()).pipe(
      tap((response) => {
        this.authSessionService.setSession(response.accessToken);
      }),
      switchMap(() => this.authSessionService.loadCurrentUser()),
      tap((user) => {
        this.authSessionService.setCurrentUser(user);
      }),
      finalize(() => {
        this.isSubmitting.set(false);
      })
    ).subscribe({
      next: () => {
        this.router.navigateByUrl('/me');
      },
      error: () => {
        this.errorMessage.set('Não foi possível fazer login.');
      }
    });
  }
}