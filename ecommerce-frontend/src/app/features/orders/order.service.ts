import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OrderItem {
  orderItemId: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface Order {
  orderId: number;
  userId: number;
  addressId: number;
  totalAmount: number;
  orderStatus: string;
  paymentStatus: string;
  paymentMethod: string;
  createdAt: string;
  orderItems: OrderItem[];
}

export interface OrdersResponse {
  data: Order[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getOrderById(orderId: number): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/api/Order/${orderId}`);
  }

 getMyOrders(): Observable<OrdersResponse> {
  return this.http.get<OrdersResponse>(`${this.apiUrl}/api/Order/myorders`);
}
cancelOrder(orderId: number): Observable<any> {
  return this.http.put(`${this.apiUrl}/api/Order/${orderId}/cancel`, {}, { responseType: 'text' });
}

downloadInvoice(orderId: number): Observable<Blob> {
  return this.http.get(`${this.apiUrl}/api/Order/${orderId}/invoice`, {
    responseType: 'blob'
  });
}
getOrderStatusHistory(orderId: number): Observable<any[]> {
  return this.http.get<any[]>(`${this.apiUrl}/api/Order/${orderId}/history`);
}
}