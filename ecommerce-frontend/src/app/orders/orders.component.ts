import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { OrderService, Order } from '../features/orders/order.service';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.scss']
})
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
  this.orderService.getMyOrders().subscribe({
    next: (res) => {
      this.orders = res.data;
      this.isLoading = false;
    },
    error: (err: any) => {
      this.errorMessage = 'Failed to load orders.';
      this.isLoading = false;
    }
  });
}

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'pending': return 'badge-pending';
      case 'confirmed': return 'badge-confirmed';
      case 'shipped': return 'badge-shipped';
      case 'delivered': return 'badge-delivered';
      case 'cancelled': return 'badge-cancelled';
      default: return 'badge-default';
    }
  }
}