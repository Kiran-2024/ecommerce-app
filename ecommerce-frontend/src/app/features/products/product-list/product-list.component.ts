import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';
import { NgxPaginationModule } from 'ngx-pagination';
import { ProductCardComponent } from '../product-card/product-card.component';
import { ProductService, Product, Category, ProductFilterParams } from '../product.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgxSpinnerModule,
    NgxPaginationModule,
    ProductCardComponent
  ],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit {

  products: Product[] = [];
  categories: Category[] = [];
  totalCount = 0;
  currentPage = 1;
  pageSize = 8;

  filters: ProductFilterParams = {
    search: '',
    categoryId: undefined,
    minPrice: undefined,
    maxPrice: undefined,
    sortBy: 'name',
    sortOrder: 'asc',
    page: 1,
    pageSize: 8
  };

  constructor(
    private productService: ProductService,
    private spinner: NgxSpinnerService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  loadProducts(): void {
    this.spinner.show();
    this.filters.page = this.currentPage;
    this.filters.pageSize = this.pageSize;

    this.productService.getProducts(this.filters).subscribe({
      next: (res) => {
        this.products = res.data;
        this.totalCount = res.totalCount;
        this.spinner.hide();
      },
      error: () => {
        this.spinner.hide();
      }
    });
  }

  loadCategories(): void {
    this.productService.getCategories().subscribe({
      next: (res) => {
        this.categories = res;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadProducts();
  }

  onSortChange(sortBy: string): void {
    if (this.filters.sortBy === sortBy) {
      this.filters.sortOrder = this.filters.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.filters.sortBy = sortBy;
      this.filters.sortOrder = 'asc';
    }
    this.currentPage = 1;
    this.loadProducts();
  }

  clearFilters(): void {
    this.filters = {
      search: '',
      categoryId: undefined,
      minPrice: undefined,
      maxPrice: undefined,
      sortBy: 'name',
      sortOrder: 'asc',
      page: 1,
      pageSize: 8
    };
    this.currentPage = 1;
    this.loadProducts();
  }
}