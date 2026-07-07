import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AdminUser {
  userId: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: string;
  isActive: boolean;
  isEmailVerified: boolean;
  createdAt: string;
}

export interface AdminRole {
  roleId: number;
  roleName: string;
}

export interface GetUsersParams {
  page: number;
  pageSize: number;
  search?: string;
  role?: string;
}

@Injectable({ providedIn: 'root' })
export class AdminUserService {
  private baseUrl = `${environment.apiUrl}/api/admin`;

  constructor(private http: HttpClient) {}

  getUsers(params: GetUsersParams): Observable<{ users: AdminUser[]; totalCount: number; totalPages: number }> {
    let query = `?page=${params.page}&pageSize=${params.pageSize}`;
    if (params.search) query += `&search=${encodeURIComponent(params.search)}`;
    if (params.role) query += `&role=${encodeURIComponent(params.role)}`;

    return this.http.get<{ data: AdminUser[]; totalCount: number; page: number; pageSize: number }>(
      `${this.baseUrl}/users${query}`
    ).pipe(
      map(res => ({
        users: res.data,
        totalCount: res.totalCount,
        totalPages: Math.ceil(res.totalCount / params.pageSize)
      }))
    );
  }

  updateStatus(userId: number, isActive: boolean): Observable<any> {
    return this.http.put(`${this.baseUrl}/users/${userId}/activate-deactivate`, { isActive });
  }

  updateRole(userId: number, roleId: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/users/${userId}/role`, { roleId });
  }

  getRoles(): Observable<AdminRole[]> {
    return this.http.get<AdminRole[]>(`${this.baseUrl}/roles`);
  }
}