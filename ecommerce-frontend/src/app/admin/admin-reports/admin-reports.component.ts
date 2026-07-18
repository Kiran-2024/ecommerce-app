import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportsService, SalesReport, CategoryRevenue, OrderStatusSummary } from '../../core/services/reports.service';

@Component({
  selector: 'app-admin-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-reports.component.html',
  styleUrls: ['./admin-reports.component.css']
})
export class AdminReportsComponent implements OnInit {
  activeTab: 'sales' | 'category' | 'status' = 'sales';

  startDate: string = '';
  endDate: string = '';

  salesData: SalesReport[] = [];
  categoryData: CategoryRevenue[] = [];
  statusData: OrderStatusSummary[] = [];

  loading = false;
  errorMsg = '';

  constructor(private reportsService: ReportsService) {}

  ngOnInit(): void {
    // Default: last 30 days
    const today = new Date();
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(today.getDate() - 30);

    this.endDate = this.formatDate(today);
    this.startDate = this.formatDate(thirtyDaysAgo);

    this.loadAllReports();
  }

  formatDate(d: Date): string {
    return d.toISOString().split('T')[0];
  }

  setTab(tab: 'sales' | 'category' | 'status'): void {
    this.activeTab = tab;
  }

  applyFilter(): void {
    this.loadAllReports();
  }

  loadAllReports(): void {
    this.loading = true;
    this.errorMsg = '';

    this.reportsService.getSalesReport(this.startDate, this.endDate).subscribe({
      next: (data) => { this.salesData = data; this.loading = false; },
      error: (err) => { this.errorMsg = 'Failed to load sales report'; this.loading = false; }
    });

    this.reportsService.getRevenueByCategory(this.startDate, this.endDate).subscribe({
      next: (data) => { this.categoryData = data; },
      error: (err) => { this.errorMsg = 'Failed to load category report'; }
    });

    this.reportsService.getOrderStatusSummary(this.startDate, this.endDate).subscribe({
      next: (data) => { this.statusData = data; },
      error: (err) => { this.errorMsg = 'Failed to load status report'; }
    });
  }

  // ---- CSV Export ----
  exportSalesCsv(): void {
    const headers = ['Order Date', 'Total Orders', 'Total Revenue', 'Avg Order Value'];
    const rows = this.salesData.map(r => [r.orderDate.split('T')[0], r.totalOrders, r.totalRevenue, r.avgOrderValue]);
    this.downloadCsv(headers, rows, 'sales-report.csv');
  }

  exportCategoryCsv(): void {
    const headers = ['Category', 'Total Orders', 'Units Sold', 'Total Revenue'];
    const rows = this.categoryData.map(r => [r.categoryName, r.totalOrders, r.unitsSold, r.totalRevenue]);
    this.downloadCsv(headers, rows, 'category-revenue-report.csv');
  }

  exportStatusCsv(): void {
    const headers = ['Status', 'Order Count', 'Total Amount'];
    const rows = this.statusData.map(r => [r.status, r.orderCount, r.totalAmount]);
    this.downloadCsv(headers, rows, 'order-status-report.csv');
  }

  private downloadCsv(headers: string[], rows: any[][], filename: string): void {
    let csvContent = headers.join(',') + '\n';
    rows.forEach(row => {
      csvContent += row.map(val => `"${val}"`).join(',') + '\n';
    });

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    URL.revokeObjectURL(url);
  }
}