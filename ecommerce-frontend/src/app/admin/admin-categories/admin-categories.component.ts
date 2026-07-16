import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminCategoryService, Category, CreateCategoryDto, UpdateCategoryDto } from '../../core/services/admin-category.service';

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-categories.component.html',
  styleUrl: './admin-categories.component.css'
})
export class AdminCategoriesComponent implements OnInit {
  categories: Category[] = [];
  isLoading = false;
  errorMsg = '';

  // Modal state
  showModal = false;
  isEditMode = false;
  selectedCategoryId: number | null = null;

  formData: { name: string; description: string; isActive: boolean } = {
    name: '',
    description: '',
    isActive: true
  };

  // Delete confirmation
  showDeleteConfirm = false;
  categoryToDelete: Category | null = null;

  constructor(private categoryService: AdminCategoryService) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading = true;
    this.categoryService.getAll().subscribe({
      next: (data) => {
        this.categories = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMsg = 'Failed to load categories';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  openAddModal(): void {
    this.isEditMode = false;
    this.selectedCategoryId = null;
    this.formData = { name: '', description: '', isActive: true };
    this.showModal = true;
  }

  openEditModal(category: Category): void {
    this.isEditMode = true;
    this.selectedCategoryId = category.categoryId;
    this.formData = {
      name: category.name,
      description: category.description,
      isActive: category.isActive
    };
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
  }

  saveCategory(): void {
    if (!this.formData.name || this.formData.name.trim() === '') {
      alert('Category name is required');
      return;
    }

    if (this.isEditMode && this.selectedCategoryId !== null) {
      const dto: UpdateCategoryDto = {
        name: this.formData.name,
        description: this.formData.description,
        isActive: this.formData.isActive
      };
      this.categoryService.update(this.selectedCategoryId, dto).subscribe({
        next: () => {
          this.closeModal();
          this.loadCategories();
        },
        error: (err) => {
          alert('Failed to update category');
          console.error(err);
        }
      });
    } else {
      const dto: CreateCategoryDto = {
        name: this.formData.name,
        description: this.formData.description
      };
      this.categoryService.create(dto).subscribe({
        next: () => {
          this.closeModal();
          this.loadCategories();
        },
        error: (err) => {
          alert('Failed to create category');
          console.error(err);
        }
      });
    }
  }

  toggleActive(category: Category): void {
    const dto: UpdateCategoryDto = {
      name: category.name,
      description: category.description,
      isActive: !category.isActive
    };
    this.categoryService.update(category.categoryId, dto).subscribe({
      next: () => {
        this.loadCategories();
      },
      error: (err) => {
        alert('Failed to update status');
        console.error(err);
      }
    });
  }

  confirmDelete(category: Category): void {
    this.categoryToDelete = category;
    this.showDeleteConfirm = true;
  }

  cancelDelete(): void {
    this.showDeleteConfirm = false;
    this.categoryToDelete = null;
  }

  deleteCategory(): void {
    if (!this.categoryToDelete) return;
    this.categoryService.delete(this.categoryToDelete.categoryId).subscribe({
      next: () => {
        this.showDeleteConfirm = false;
        this.categoryToDelete = null;
        this.loadCategories();
      },
      error: (err) => {
        alert('Failed to delete category');
        console.error(err);
      }
    });
  }
}