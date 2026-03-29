import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { AuthSessionService } from './features/auth/services/auth-session.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  private readonly authSessionService = inject(AuthSessionService);

  readonly isRestoringSession = this.authSessionService.isRestoringSession;

  ngOnInit(): void {
    this.authSessionService.restoreSession();
  }
}