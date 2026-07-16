import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Category {
  categoryId: number;
  name: string;
  description: string;
  isActive: boolean;
}

export interface CreateCategoryDto {
  name: string;
  description: string;
}

export interface UpdateCategoryDto {
  name: string;
  description: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AdminCategoryService {
  private baseUrl = `${environment.apiUrl}/api/categories`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Category[]> {
  return this.http.get<Category[]>(`${this.baseUrl}/admin/all`);
}

  getById(id: number): Observable<Category> {
    return this.http.get<Category>(`${this.baseUrl}/${id}`);
  }

  create(dto: CreateCategoryDto): Observable<any> {
    return this.http.post<any>(this.baseUrl, dto);
  }

  update(id: number, dto: UpdateCategoryDto): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`);
  }
  
}