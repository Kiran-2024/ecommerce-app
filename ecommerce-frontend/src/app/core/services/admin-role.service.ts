import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Role {
  roleId: number;
  roleName: string;
}

export interface Right {
  rightId: number;
  rightName: string;
  description: string;
}

@Injectable({ providedIn: 'root' })
export class AdminRoleService {
  private api = `${environment.apiUrl}/api/admin`;

  constructor(private http: HttpClient) {}

  getAllRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(`${this.api}/roles`);
  }

  createRole(roleName: string): Observable<any> {
    return this.http.post(`${this.api}/roles`, { roleName });
  }

  updateRole(roleId: number, roleName: string): Observable<any> {
    return this.http.put(`${this.api}/roles/${roleId}`, { roleName });
  }

  deleteRole(roleId: number): Observable<any> {
    return this.http.delete(`${this.api}/roles/${roleId}`);
  }

  getRoleRights(roleId: number): Observable<Right[]> {
    return this.http.get<Right[]>(`${this.api}/roles/${roleId}/rights`);
  }

  assignRights(roleId: number, rightIds: number[]): Observable<any> {
    return this.http.post(`${this.api}/roles/${roleId}/rights`, { rightIds });
  }

  getAllRights(): Observable<Right[]> {
    return this.http.get<Right[]>(`${this.api}/rights`);
  }
}