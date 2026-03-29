import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { CurrentUserResponse } from '../../domain/auth.models';
import { AuthSessionService } from '../../services/auth-session.service';

@Component({
  selector: 'app-me',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './me.html',
  styleUrl: './me.scss'
})
export class Me implements OnInit {
  private readonly authSessionService = inject(AuthSessionService);
  private readonly router = inject(Router);

  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly user = signal<CurrentUserResponse | null>(null);

  ngOnInit(): void {
    this.authSessionService.loadCurrentUser().subscribe({
      next: (user) => {
        this.authSessionService.setCurrentUser(user);
        this.user.set(user);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Não foi possível carregar o usuário autenticado.');
        this.isLoading.set(false);
      }
    });
  }

  logout(): void {
    this.authSessionService.clearSession();
    this.router.navigateByUrl('/login');
  }
}