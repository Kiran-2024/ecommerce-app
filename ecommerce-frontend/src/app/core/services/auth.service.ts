import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TokenHelper } from '../helpers/token.helpers';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private apiUrl = environment.apiUrl + '/auth';
  private readonly TOKEN_KEY = 'jwt_token';
  private readonly REFRESH_KEY = 'refresh_token';

  constructor(private http: HttpClient, private router: Router) {}

  register(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, dto);
  }

  verifyOtp(data: { email: string; otp: string; type: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/verify-otp`, data);
  }

  resendOtp(data: { email: string; type: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/resend-otp`, data);
  }

  login(dto: { email: string; password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, dto).pipe(
      tap((res: any) => {
        if (res?.token) {
          localStorage.setItem(this.TOKEN_KEY, res.token);
          localStorage.setItem(this.REFRESH_KEY, res.refreshToken);
        }
      })
    );
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(dto: { email: string; otp: string; newPassword: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, dto);
  }

  refreshToken(): Observable<any> {
    const refreshToken = localStorage.getItem(this.REFRESH_KEY);
    return this.http.post(`${this.apiUrl}/refresh-token`, { refreshToken }).pipe(
      tap((res: any) => {
        if (res?.token) {
          localStorage.setItem(this.TOKEN_KEY, res.token);
          localStorage.setItem(this.REFRESH_KEY, res.refreshToken);
        }
      })
    );
  }

  logout(): void {
    const refreshToken = localStorage.getItem(this.REFRESH_KEY);
    this.http.post(`${this.apiUrl}/logout`, { refreshToken }).subscribe();
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    return !TokenHelper.isTokenExpired(token);
  }

  getCurrentUser(): any {
    const token = this.getToken();
    if (!token) return null;
    return {
      userId: TokenHelper.getUserId(token),
      email: TokenHelper.getEmail(token),
      role: TokenHelper.getRole(token),
      rights: TokenHelper.getRights(token)
    };
  }

  getRole(): string | null {
    const token = this.getToken();
    return token ? TokenHelper.getRole(token) : null;
  }

  hasRight(right: string): boolean {
    const token = this.getToken();
    if (!token) return false;
    return TokenHelper.getRights(token).includes(right);
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }
}