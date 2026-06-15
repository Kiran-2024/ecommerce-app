import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PlaceOrderRequest {
  addressId: number;
  paymentMethod: 'COD' | 'Online';
  notes?: string;
}

export interface PlaceOrderResponse {
  orderId: number;
  totalAmount: number;
  paymentMethod: string;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class CheckoutService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  placeOrder(request: PlaceOrderRequest): Observable<PlaceOrderResponse> {
    return this.http.post<PlaceOrderResponse>(`${this.apiUrl}/api/orders`, request);
  }
}