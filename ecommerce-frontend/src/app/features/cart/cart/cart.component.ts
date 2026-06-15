import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { CartService, CartItem, CartResponse } from '../cart.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss']
})
export class CartComponent implements OnInit {
  items: CartItem[] = [];
  total: number = 0;
  loading = false;

  constructor(private cartService: CartService,
    private router:Router
  ) {}

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.loading = true;
    this.cartService.getCart().subscribe({
      next: (res: CartResponse) => {
        this.items = res.items;
        this.total = res.total;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  updateQty(cartItemId: number, quantity: number): void {
    if (quantity < 1) return;
    this.cartService.updateQty(cartItemId, quantity).subscribe({
      next: () => this.loadCart()
    });
  }

  removeItem(cartItemId: number): void {
    this.cartService.removeItem(cartItemId).subscribe({
      next: () => this.loadCart()
    });
  }

  clearCart(): void {
    this.cartService.clearCart().subscribe({
      next: () => {
        this.items = [];
        this.total = 0;
      }
    });
  }

  getImageUrl(imageUrl: string | null): string {
    if (!imageUrl) return 'assets/images/no-image.png';
    return `${environment.apiUrl}${imageUrl}`;
  }

  goToCheckout(): void {
  this.router.navigate(['/checkout']);
  }
}