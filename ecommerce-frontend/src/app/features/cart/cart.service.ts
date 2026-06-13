import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CartItem {
  cartItemId: number;
  productId: number;
  productName: string;
  imageUrl: string | null;
  price: number;
  quantity: number;
  subtotal: number;
  addedAt: string;
}

export interface CartResponse {
  items: CartItem[];
  total: number;
}

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiUrl = `${environment.apiUrl}/api/cart`;

  // Cart badge count ki BehaviorSubject
  private cartCountSubject = new BehaviorSubject<number>(0);
  cartCount$ = this.cartCountSubject.asObservable();

  constructor(private http: HttpClient) {}

  getCart(): Observable<CartResponse> {
    return this.http.get<CartResponse>(this.apiUrl).pipe(
      tap(res => this.cartCountSubject.next(res.items.length))
    );
  }

  addItem(productId: number, quantity: number = 1): Observable<any> {
    return this.http.post(`${this.apiUrl}/add`, { productId, quantity }).pipe(
      tap(() => this.cartCountSubject.next(this.cartCountSubject.value + 1))
    );
  }

  updateQty(cartItemId: number, quantity: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/update/${cartItemId}`, { quantity });
  }

  removeItem(cartItemId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/remove/${cartItemId}`).pipe(
      tap(() => this.cartCountSubject.next(this.cartCountSubject.value - 1))
    );
  }

  clearCart(): Observable<any> {
    return this.http.delete(`${this.apiUrl}/clear`).pipe(
      tap(() => this.cartCountSubject.next(0))
    );
  }

  updateCartCount(count: number): void {
    this.cartCountSubject.next(count);
  }
}