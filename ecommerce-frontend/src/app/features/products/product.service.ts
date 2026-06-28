import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Product {
  productId: number;
  productName: string;      // ← name కాదు, productName!
  description: string;
  price: number;
  discountPrice: number;    // ← ఇది add చేయి
  stock: number;
  categoryId: number;
  categoryName: string;
  imageUrl: string;
  isActive: boolean;
  createdAt: string;
}

export interface Category {
  categoryId: number;
  name: string;
}

export interface ProductFilterParams {
  search?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: string;
  sortOrder?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private apiUrl = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) {}

  getProducts(filters: ProductFilterParams = {}): Observable<PagedResult<Product>> {
    let params = new HttpParams();

    if (filters.search) params = params.set('search', filters.search);
    if (filters.categoryId) params = params.set('categoryId', filters.categoryId.toString());
    if (filters.minPrice !== undefined) params = params.set('minPrice', filters.minPrice.toString());
    if (filters.maxPrice !== undefined) params = params.set('maxPrice', filters.maxPrice.toString());
    if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
    if (filters.sortOrder) params = params.set('sortOrder', filters.sortOrder);
    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());

    return this.http.get<PagedResult<Product>>(`${this.apiUrl}/products`, { params });
  }

  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/products/${id}`);
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.apiUrl}/categories`);
  }
  createProduct(data: any): Observable<any> {
  return this.http.post(`${this.apiUrl}/products`, data);
}

updateProduct(id: number, data: any): Observable<any> {
  return this.http.put(`${this.apiUrl}/products/${id}`, data);
}

deleteProduct(id: number): Observable<any> {
  return this.http.delete(`${this.apiUrl}/products/${id}`);
}

uploadImage(file: File): Observable<any> {
  const formData = new FormData();
  formData.append('file', file);
  return this.http.post(`${this.apiUrl}/products/upload-image`, formData);
}
}