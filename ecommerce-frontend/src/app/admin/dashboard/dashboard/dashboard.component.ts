import { Component, OnInit, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, registerables } from 'chart.js';
import { AdminDashboardService, DashboardStats } from '../../../core/services/admin-dashboard.service';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, AfterViewInit {
  @ViewChild('orderStatusChart') chartRef!: ElementRef<HTMLCanvasElement>;

  stats: DashboardStats | null = null;
  loading = true;
  error = '';
  private chartInstance: Chart | null = null;

  constructor(private dashboardService: AdminDashboardService) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  ngAfterViewInit(): void {
    // Chart renders after data loads (see renderChart call in loadDashboard)
  }

  loadDashboard(): void {
    this.loading = true;
    this.dashboardService.getDashboardStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.loading = false;
        setTimeout(() => this.renderChart(), 0); // wait for canvas to be in DOM
      },
      error: (err) => {
        console.error('Dashboard load error:', err);
        this.error = 'Failed to load dashboard data';
        this.loading = false;
      }
    });
  }

  renderChart(): void {
    if (!this.stats || !this.chartRef) return;

    const others = this.stats.totalOrders - this.stats.pendingOrders - this.stats.cancelledOrders;

    if (this.chartInstance) {
      this.chartInstance.destroy();
    }

    this.chartInstance = new Chart(this.chartRef.nativeElement, {
      type: 'doughnut',
      data: {
        labels: ['Pending', 'Cancelled', 'Others (Shipped/Delivered)'],
        datasets: [{
          data: [this.stats.pendingOrders, this.stats.cancelledOrders, others],
          backgroundColor: ['#e9c46a', '#e63946', '#2a9d8f'],
          borderWidth: 0
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom'
          }
        }
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pending': return 'badge-pending';
      case 'Shipped': return 'badge-shipped';
      case 'Delivered': return 'badge-delivered';
      case 'Cancelled': return 'badge-cancelled';
      default: return 'badge-default';
    }
  }
}