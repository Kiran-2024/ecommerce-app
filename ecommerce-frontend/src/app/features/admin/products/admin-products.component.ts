import { Component, OnInit,Inject} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductService } from '../../products/product.service';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NgxSpinnerModule],
  templateUrl: './admin-products.component.html',
  styleUrls: ['./admin-products.component.scss']
})
export class AdminProductsComponent implements OnInit {
  products: any[] = [];
  categories: any[] = [];
  totalCount = 0;
  currentPage = 1;
  pageSize = 10;
  searchTerm = '';
  selectedCategory: number | string = '';

  showModal = false;
  isEditMode = false;
  selectedProductId: number | null = null;
  showDeleteConfirm = false;
  deleteProductId: number | null = null;

  selectedFile: File | null = null;
  previewUrl: string | null = null;

  productForm: FormGroup;

  constructor(
    private productService: ProductService,
    private fb: FormBuilder,
    private spinner: NgxSpinnerService,
     @Inject(ToastrService) private toastr: ToastrService
  ) {
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      price: [0, [Validators.required, Validators.min(1)]],
      stock: [0, [Validators.required, Validators.min(0)]],
      categoryId: ['', Validators.required],
      imageUrl: ['']
    });
  }

  ngOnInit() {
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories() {
    this.productService.getCategories().subscribe({
      next: (res: any) => this.categories = res,
      error: () => this.toastr.error('Categories load failed')
    });
  }

  loadProducts() {
    this.spinner.show();
    this.productService.getProducts({
      page: this.currentPage,
      pageSize: this.pageSize,
      search: this.searchTerm,
      categoryId: this.selectedCategory as number
    }).subscribe({
      next: (res: any) => {
        this.products = res.data || res.items || res;
        this.totalCount = res.totalCount || this.products.length;
        this.spinner.hide();
      },
      error: () => {
        this.toastr.error('Products load failed');
        this.spinner.hide();
      }
    });
  }

  onSearch() {
    this.currentPage = 1;
    this.loadProducts();
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadProducts();
  }

  openAddModal() {
    this.isEditMode = false;
    this.selectedProductId = null;
    this.productForm.reset({ price: 0, stock: 0 });
    this.previewUrl = null;
    this.selectedFile = null;
    this.showModal = true;
  }

  openEditModal(product: any) {
    this.isEditMode = true;
    this.selectedProductId = product.productId;
    this.productForm.patchValue({
      name: product.productName,
      description: product.description,
      price: product.price,
      stock: product.stock,
      categoryId: product.categoryId,
      imageUrl: product.imageUrl
    });
    this.previewUrl = product.imageUrl;
    this.selectedFile = null;
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
  }

  onFileChange(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = (e: any) => this.previewUrl = e.target.result;
      reader.readAsDataURL(file);
    }
  }

  saveProduct() {
    if (this.productForm.invalid) return;

    const saveAction = () => {
      const data = this.productForm.value;
      this.spinner.show();

      if (this.isEditMode && this.selectedProductId) {
        this.productService.updateProduct(this.selectedProductId, data).subscribe({
          next: () => {
            this.toastr.success('Product updated!');
            this.closeModal();
            this.loadProducts();
            this.spinner.hide();
          },
          error: () => { this.toastr.error('Update failed'); this.spinner.hide(); }
        });
      } else {
        this.productService.createProduct(data).subscribe({
          next: () => {
            this.toastr.success('Product created!');
            this.closeModal();
            this.loadProducts();
            this.spinner.hide();
          },
          error: () => { this.toastr.error('Create failed'); this.spinner.hide(); }
        });
      }
    };

    if (this.selectedFile) {
      this.productService.uploadImage(this.selectedFile).subscribe({
        next: (res: any) => {
          this.productForm.patchValue({ imageUrl: res.imageUrl });
          saveAction();
        },
        error: () => { this.toastr.error('Image upload failed'); }
      });
    } else {
      saveAction();
    }
  }

  confirmDelete(id: number) {
    this.deleteProductId = id;
    this.showDeleteConfirm = true;
  }

  cancelDelete() {
    this.showDeleteConfirm = false;
    this.deleteProductId = null;
  }

  deleteProduct() {
    if (!this.deleteProductId) return;
    this.spinner.show();
    this.productService.deleteProduct(this.deleteProductId).subscribe({
      next: () => {
        this.toastr.success('Product deleted!');
        this.showDeleteConfirm = false;
        this.deleteProductId = null;
        this.loadProducts();
        this.spinner.hide();
      },
      error: () => { this.toastr.error('Delete failed'); this.spinner.hide(); }
    });
  }

  get totalPages() {
    return Math.ceil(this.totalCount / this.pageSize);
  }
}