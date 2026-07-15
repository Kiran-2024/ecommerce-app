import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface RecentOrder {
  orderId: number;
  customerName: string;
  totalAmount: number;
  orderStatus: string;
  createdAt: string;
}

export interface LowStockProduct {
  productId: number;
  productName: string;
  stockQuantity: number;
}

export interface DashboardStats {
  totalOrders: number;
  totalRevenue: number;
  totalUsers: number;
  totalProducts: number;
  pendingOrders: number;
  cancelledOrders: number;
  recentOrders: RecentOrder[];
  lowStockProducts: LowStockProduct[];
}

@Injectable({
  providedIn: 'root'
})
export class AdminDashboardService {
  private apiUrl = `${environment.apiUrl}/api/admin/dashboard`;

  constructor(private http: HttpClient) {}

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(this.apiUrl);
  }
}