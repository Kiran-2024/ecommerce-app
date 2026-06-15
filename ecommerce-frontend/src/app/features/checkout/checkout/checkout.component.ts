import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../cart/cart.service';
import { AddressService } from '../../address/address.service';
import { CheckoutService, PlaceOrderRequest } from '../checkout.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.scss']
})
export class CheckoutComponent implements OnInit {

  cartItems: any[] = [];
  addresses: any[] = [];
  selectedAddressId: number | null = null;
  paymentMethod: 'COD' | 'Online' = 'COD';
  subtotal: number = 0;
  isLoading: boolean = false;
  errorMsg: string = '';

  constructor(
    private cartService: CartService,
    private addressService: AddressService,
    private checkoutService: CheckoutService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCart();
    this.loadAddresses();
  }

  loadCart(): void {
    this.cartService.getCart().subscribe({
      next: (res) => {
        this.cartItems = res.items || [];
        this.subtotal = this.cartItems.reduce(
          (sum, item) => sum + item.price * item.quantity, 0
        );
      },
      error: () => { this.errorMsg = 'Failed to load cart.'; }
    });
  }

  loadAddresses(): void {
    this.addressService.getAddresses().subscribe({
      next: (res) => {
        this.addresses = res;
        const def = res.find((a: any) => a.isDefault);
        if (def) this.selectedAddressId = def.addressId;
      },
      error: () => { this.errorMsg = 'Failed to load addresses.'; }
    });
  }

  selectAddress(id: number): void {
    this.selectedAddressId = id;
  }

  getImageUrl(imageUrl: string): string {
    if (!imageUrl) return 'assets/no-image.png';
    if (imageUrl.startsWith('http')) return imageUrl;
    return `${environment.apiUrl}/${imageUrl}`;
  }

  goToAddressBook(): void {
    this.router.navigate(['/profile/addresses']);
  }

  placeOrder(): void {
    if (!this.selectedAddressId) {
      this.errorMsg = 'Please select a delivery address.';
      return;
    }

    this.isLoading = true;
    this.errorMsg = '';

    const request: PlaceOrderRequest = {
      addressId: this.selectedAddressId,
      paymentMethod: this.paymentMethod
    };

    this.checkoutService.placeOrder(request).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.router.navigate(['/orders', res.orderId]);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMsg = err.error?.message || 'Failed to place order. Try again.';
      }
    });
  }
}