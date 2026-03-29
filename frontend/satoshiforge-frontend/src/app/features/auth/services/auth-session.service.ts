import { Injectable, computed, inject, signal } from '@angular/core';
import { tap } from 'rxjs';

import {
  CurrentUserResponse,
  LoginUserRequest,
  RegisterUserRequest
} from '../domain/auth.models';
import { AuthApiService } from '../data/auth-api.service';
import { TokenStorageService } from './token-storage.service';

@Injectable({
  providedIn: 'root'
})
export class AuthSessionService {
  private readonly authApiService = inject(AuthApiService);
  private readonly tokenStorageService = inject(TokenStorageService);

  private readonly accessTokenSignal = signal<string | null>(this.tokenStorageService.getToken());
  private readonly currentUserSignal = signal<CurrentUserResponse | null>(null);
  private readonly isRestoringSessionSignal = signal(false);
  private readonly hasAttemptedRestoreSignal = signal(false);

  readonly accessToken = computed(() => this.accessTokenSignal());
  readonly currentUser = computed(() => this.currentUserSignal());
  readonly isRestoringSession = computed(() => this.isRestoringSessionSignal());
  readonly hasAttemptedRestore = computed(() => this.hasAttemptedRestoreSignal());

  readonly hasToken = computed(() => !!this.accessTokenSignal());
  readonly isAuthenticated = computed(() => !!this.currentUserSignal());

  register(payload: RegisterUserRequest) {
    return this.authApiService.register(payload);
  }

  login(payload: LoginUserRequest) {
    return this.authApiService.login(payload);
  }

  setSession(token: string): void {
    this.tokenStorageService.setToken(token);
    this.accessTokenSignal.set(token);
  }

  clearSession(): void {
    this.tokenStorageService.clearToken();
    this.accessTokenSignal.set(null);
    this.currentUserSignal.set(null);
  }

  setCurrentUser(user: CurrentUserResponse): void {
    this.currentUserSignal.set(user);
  }

  loadCurrentUser() {
    return this.authApiService.me().pipe(
      tap((user) => {
        this.currentUserSignal.set(user);
      })
    );
  }

  restoreSession(): void {
    const token = this.tokenStorageService.getToken();

    if (!token) {
      this.hasAttemptedRestoreSignal.set(true);
      return;
    }

    this.isRestoringSessionSignal.set(true);

    this.loadCurrentUser().subscribe({
      next: () => {
        this.isRestoringSessionSignal.set(false);
        this.hasAttemptedRestoreSignal.set(true);
      },
      error: () => {
        this.clearSession();
        this.isRestoringSessionSignal.set(false);
        this.hasAttemptedRestoreSignal.set(true);
      }
    });
  }
}