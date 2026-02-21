import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app/app.routes';
import { authInterceptor } from './app/interceptors/auth.interceptor';
import { SOCIAL_AUTH_CONFIG, GoogleLoginProvider } from '@abacritt/angularx-social-login';

try {
  await bootstrapApplication(AppComponent, {
    providers: [
      provideRouter(routes),
      provideHttpClient(withInterceptors([authInterceptor])),
      {
        provide: SOCIAL_AUTH_CONFIG,
        useValue: {
          autoLogin: false,
          providers: [
            {
              id: GoogleLoginProvider.PROVIDER_ID,
              provider: new GoogleLoginProvider(
                '371391155525-lv8ismho1cvq43vddljba6r23ilktv2q.apps.googleusercontent.com',
                { oneTapEnabled: false }
              )
            }
          ]
        }
      }
    ]
  });
} catch (err) {
  console.error(err);
}
