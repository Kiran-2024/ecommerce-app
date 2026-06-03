import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class NavigationService {

  constructor(
    private router: Router,
    private location: Location,
    private authService: AuthService
  ) {}

  goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  goToHome(): void {
    this.router.navigate(['/home']);
  }

  goToAdmin(): void {
    this.router.navigate(['/admin']);
  }

  goToUnauthorized(): void {
    this.router.navigate(['/unauthorized']);
  }

  goBack(): void {
    this.location.back();
  }

  // Login ayinaaka role batti redirect cheyyi
  redirectAfterLogin(): void {
    if (this.authService.isAdmin()) {
      this.router.navigate(['/admin']);
    } else {
      this.router.navigate(['/home']);
    }
  }
}