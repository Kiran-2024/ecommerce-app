import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SalesReport {
  orderDate: string;
  totalOrders: number;
  totalRevenue: number;
  avgOrderValue: number;
}

export interface CategoryRevenue {
  categoryId: number;
  categoryName: string;
  totalOrders: number;
  unitsSold: number;
  totalRevenue: number;
}

export interface OrderStatusSummary {
  status: string;
  orderCount: number;
  totalAmount: number;
}

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private baseUrl = `${environment.apiUrl}/api/admin/reports`;

  constructor(private http: HttpClient) {}

  getSalesReport(startDate?: string, endDate?: string): Observable<SalesReport[]> {
    let params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    return this.http.get<SalesReport[]>(`${this.baseUrl}/sales`, { params });
  }

  getRevenueByCategory(startDate?: string, endDate?: string): Observable<CategoryRevenue[]> {
    let params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    return this.http.get<CategoryRevenue[]>(`${this.baseUrl}/revenue-by-category`, { params });
  }

  getOrderStatusSummary(startDate?: string, endDate?: string): Observable<OrderStatusSummary[]> {
    let params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    return this.http.get<OrderStatusSummary[]>(`${this.baseUrl}/order-status`, { params });
  }
}