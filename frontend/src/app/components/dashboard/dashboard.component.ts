import { Component, signal, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { WeatherService } from '../../services/weather.service';
import { WeatherData } from '../../models/weather.model';
import { SocialAuthService } from '@abacritt/angularx-social-login';
import * as L from 'leaflet';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements AfterViewInit, OnDestroy {
  city = signal('');
  weatherData = signal<WeatherData | null>(null);
  error = signal('');
  loading = signal(false);

  private map: L.Map | null = null;
  private marker: L.CircleMarker | null = null;

  constructor(
    private readonly authService: AuthService,
    private readonly socialAuthService: SocialAuthService,
    private readonly weatherService: WeatherService,
    private readonly router: Router
  ) {}

  get user() {
    return this.authService.user;
  }

  ngAfterViewInit(): void {
    this.map = L.map('weather-map').setView([20, 0], 2);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      maxZoom: 18
    }).addTo(this.map);
  }

  ngOnDestroy(): void {
    if (this.map) {
      this.map.remove();
      this.map = null;
    }
  }

  private updateMap(lat: number, lon: number): void {
    if (!this.map) return;
    this.map.invalidateSize();
    this.map.setView([lat, lon], 10);
    if (this.marker) {
      this.marker.setLatLng([lat, lon]);
    } else {
      this.marker = L.circleMarker([lat, lon], {
        radius: 9,
        color: '#1a73e8',
        fillColor: '#4a9edd',
        fillOpacity: 0.8,
        weight: 2
      }).addTo(this.map);
    }
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
        setTimeout(() => this.updateMap(data.latitude, data.longitude), 50);
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
