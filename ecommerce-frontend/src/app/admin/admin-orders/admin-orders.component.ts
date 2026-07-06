import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminOrderService, AdminOrder } from '../../core/services/admin-order.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-orders.component.html',
  styleUrls: ['./admin-orders.component.css']
})
export class AdminOrdersComponent implements OnInit {
  orders: AdminOrder[] = [];
  totalCount = 0;
  totalPages = 0;
  currentPage = 1;
  pageSize = 10;

  // Filters
  selectedStatus = '';
  searchText = '';
  fromDate = '';
  toDate = '';

  statuses = ['Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled'];
  loading = false;

  constructor(
    private adminOrderService: AdminOrderService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.adminOrderService.getOrders({
      page: this.currentPage,
      pageSize: this.pageSize,
      status: this.selectedStatus || undefined,
      search: this.searchText || undefined,
      fromDate: this.fromDate || undefined,
      toDate: this.toDate || undefined
    }).subscribe({
      next: (res) => {
        this.orders = res.orders;
        this.totalCount = res.totalCount;
        this.totalPages = res.totalPages;
        this.loading = false;
      },
      error: () => {
        this.toastr.error('Orders load cheyyadam fail ayindi');
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadOrders();
  }

  resetFilters(): void {
    this.selectedStatus = '';
    this.searchText = '';
    this.fromDate = '';
    this.toDate = '';
    this.currentPage = 1;
    this.loadOrders();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadOrders();
  }

  updateStatus(order: AdminOrder, newStatus: string): void {
    if (order.status === newStatus) return;

    this.adminOrderService.updateStatus(order.orderId, newStatus).subscribe({
      next: () => {
        order.status = newStatus;
        this.toastr.success(`Order #${order.orderId} status updated to ${newStatus}`);
      },
      error: () => {
        this.toastr.error('Status update fail ayindi');
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pending': return 'badge-pending';
      case 'Confirmed': return 'badge-confirmed';
      case 'Shipped': return 'badge-shipped';
      case 'Delivered': return 'badge-delivered';
      case 'Cancelled': return 'badge-cancelled';
      default: return '';
    }
  }

  getPages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }
}