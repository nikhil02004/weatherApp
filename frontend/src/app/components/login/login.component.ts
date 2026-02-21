import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { SocialAuthService, GoogleSigninButtonModule } from '@abacritt/angularx-social-login';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, GoogleSigninButtonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit, OnDestroy {
  username = signal('');
  password = signal('');
  error = signal('');
  loading = signal(false);

  private authStateSub!: Subscription;

  constructor(
    private readonly authService: AuthService,
    private readonly socialAuthService: SocialAuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    // filter(!!user) ignores null emissions (init / after logout) without skipping the real first sign-in
    this.authStateSub = this.socialAuthService.authState.pipe(filter(user => !!user)).subscribe(user => {
      if (!user?.idToken) return;
      this.loading.set(true);
      this.error.set('');
      this.authService.loginWithGoogle(user.idToken).subscribe({
        next: () => {
          this.loading.set(false);
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set(err.error || 'Google login failed');
        }
      });
    });
  }

  ngOnDestroy(): void {
    this.authStateSub?.unsubscribe();
  }

  onSubmit(): void {
    if (!this.username() || !this.password()) {
      this.error.set('Please fill in all fields');
      return;
    }

    this.loading.set(true);
    this.error.set('');

    this.authService.login({ username: this.username(), password: this.password() }).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error || 'Login failed');
      }
    });
  }

  goToRegister(): void {
    this.router.navigate(['/register']);
  }
}
