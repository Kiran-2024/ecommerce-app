import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';
import { ProductService, Product } from '../product.service';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, NgxSpinnerModule],
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.scss']
})
export class ProductDetailComponent implements OnInit {

  product: Product | null = null;
  relatedProducts: Product[] = [];
  selectedImage: string = '';

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private spinner: NgxSpinnerService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const id = +params['id'];
      this.loadProduct(id);
    });
  }

  loadProduct(id: number): void {
    this.spinner.show();
    this.productService.getProductById(id).subscribe({
      next: (res) => {
        this.product = res;
        this.selectedImage = res.imageUrl || '';
        this.loadRelatedProducts(res.categoryId);
        this.spinner.hide();
      },
      error: () => {
        this.spinner.hide();
      }
    });
  }

  loadRelatedProducts(categoryId: number): void {
    this.productService.getProducts({ categoryId, pageSize: 4 }).subscribe({
      next: (res) => {
        this.relatedProducts = res.data.filter(p => p.productId !== this.product?.productId);
      }
    });
  }

  getDiscountPercent(): number {
    if (!this.product || !this.product.discountPrice) return 0;
    return Math.round(((this.product.price - this.product.discountPrice) / this.product.price) * 100);
  }
}