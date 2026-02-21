import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { WeatherService } from '../../services/weather.service';
import { WeatherData } from '../../models/weather.model';
import { SocialAuthService } from '@abacritt/angularx-social-login';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent {
  city = signal('');
  weatherData = signal<WeatherData | null>(null);
  error = signal('');
  loading = signal(false);

  constructor(
    private readonly authService: AuthService,
    private readonly socialAuthService: SocialAuthService,
    private readonly weatherService: WeatherService,
    private readonly router: Router
  ) {}

  get user() {
    return this.authService.user;
  }

  onSearch(): void {
    if (!this.city()) {
      this.error.set('Please enter a city name');
      return;
    }

    this.loading.set(true);
    this.error.set('');

    this.weatherService.getWeather(this.city()).subscribe({
      next: (data) => {
        this.weatherData.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Failed to load weather data');
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.socialAuthService.signOut(true).catch(() => {});
    this.router.navigate(['/login']);
  }
}
