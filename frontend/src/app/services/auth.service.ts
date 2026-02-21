import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginRequest, RegisterRequest, AuthResponse } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = '/api/auth';
  private readonly tokenSignal = signal<string | null>(this.getToken());
  private readonly userSignal = signal<{ username: string } | null>(this.getUser());
  
  token = this.tokenSignal.asReadonly();
  user = this.userSignal.asReadonly();
  isAuthenticated = computed(() => !!this.tokenSignal());

  constructor(private readonly http: HttpClient) {}

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, {
      username: credentials.username,
      password: credentials.password
    }).pipe(tap(response => this.setAuth(response)));
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data)
      .pipe(tap(response => this.setAuth(response)));
  }

  loginWithGoogle(idToken: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/google`, {
      provider: 'Google',
      idToken
    }).pipe(tap(response => this.setAuth(response)));
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.tokenSignal.set(null);
    this.userSignal.set(null);
  }

  private setAuth(response: AuthResponse): void {
    const userData = { username: response.username };
    localStorage.setItem('token', response.token);
    localStorage.setItem('user', JSON.stringify(userData));
    this.tokenSignal.set(response.token);
    this.userSignal.set(userData);
  }

  private getToken(): string | null {
    return localStorage.getItem('token');
  }

  private getUser(): { username: string } | null {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }
}
