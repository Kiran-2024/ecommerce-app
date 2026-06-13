import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/api/auth`;
  private readonly TOKEN_KEY = 'accessToken';
  private readonly REFRESH_KEY = 'refreshToken';

  constructor(private http: HttpClient, private router: Router) {}

  register(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  verifyOtp(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/verify-email-otp`, data);
  }

  resendOtp(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/resend-otp`, { email });
  }

  login(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, data);
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, data);
  }

  refreshAccessToken(refreshToken: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/refresh-token`, { refreshToken });
  }

  logout(refreshToken: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/logout`, { refreshToken });
  }

  saveTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(this.TOKEN_KEY, accessToken);
    localStorage.setItem(this.REFRESH_KEY, refreshToken);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_KEY);
  }

  clearTokens(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }

  getRole(): string | null {
    const token = this.getAccessToken();
    if (!token) return null;
    try {
      const decoded: any = jwtDecode(token);
      return decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
    } catch {
      return null;
    }
  }

  hasRight(right: string): boolean {
    const token = this.getAccessToken();
    if (!token) return false;
    try {
      const decoded: any = jwtDecode(token);
      const rights = decoded['right'] || [];
      return rights.includes(right);
    } catch {
      return false;
    }
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }
}