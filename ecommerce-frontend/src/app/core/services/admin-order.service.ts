import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
export interface AdminOrder {
  orderId: number;
  userId: number;
  customerName: string;
  customerEmail: string;
  totalAmount: number;
  status: string;
  paymentMethod: string;
  paymentStatus: string;
  createdAt: string;
  updatedAt: string | null;
  itemCount: number;
}

export interface AdminOrdersResponse {
  orders: AdminOrder[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class AdminOrderService {
  private api = `${environment.apiUrl}/api/admin/orders`;

  constructor(private http: HttpClient) {}

  getOrders(filters: {
    page: number;
    pageSize: number;
    status?: string;
    search?: string;
    fromDate?: string;
    toDate?: string;
  }): Observable<AdminOrdersResponse> {
    let params = new HttpParams()
      .set('page', filters.page)
      .set('pageSize', filters.pageSize);

    if (filters.status) params = params.set('status', filters.status);
    if (filters.search) params = params.set('search', filters.search);
    if (filters.fromDate) params = params.set('fromDate', filters.fromDate);
    if (filters.toDate) params = params.set('toDate', filters.toDate);

    return this.http.get<AdminOrdersResponse>(this.api, { params });
  }

  updateStatus(orderId: number, status: string): Observable<any> {
    return this.http.put(`${this.api}/${orderId}/status`, { status });
  }
}