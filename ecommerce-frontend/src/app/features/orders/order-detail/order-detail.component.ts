import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { OrderService, Order } from '../order.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.scss']
})
export class OrderDetailComponent implements OnInit {

  order: Order | null = null;
  isLoading: boolean = true;
  errorMsg: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService
  ) {}

  ngOnInit(): void {
    const orderId = Number(this.route.snapshot.paramMap.get('id'));
    if (!orderId) {
      this.errorMsg = 'Invalid order id.';
      this.isLoading = false;
      return;
    }
    this.loadOrder(orderId);
  }

  loadOrder(orderId: number): void {
    this.isLoading = true;
    this.orderService.getOrderById(orderId).subscribe({
      next: (res) => {
        this.order = res;
        this.isLoading = false;
      },
      error: () => {
        this.errorMsg = 'Failed to load order details.';
        this.isLoading = false;
      }
    });
  }

  getImageUrl(imageUrl: string): string {
    if (!imageUrl) return 'https://placehold.co/60x60?text=No+Image';
    if (imageUrl.startsWith('http')) return imageUrl;
    return `${environment.apiUrl}/${imageUrl}`;
  }

  goToProducts(): void {
    this.router.navigate(['/products']);
  }

 cancelOrder(): void {
  if (!this.order) return;
  if (!confirm('Are you sure you want to cancel this order?')) return;

  this.orderService.cancelOrder(this.order.orderId).subscribe({
    next: () => {
      this.order!.orderStatus = 'Cancelled';
    },
    error: (err: any) => {
      if (err.status === 200) {
        this.order!.orderStatus = 'Cancelled';
      } else {
        alert('Failed to cancel order. Please try again.');
      }
    }
  });
}

canCancel(): boolean {
  return this.order?.orderStatus?.toLowerCase() === 'pending';
}

  
}