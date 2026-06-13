import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
@Component({
  selector: 'app-product-image-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-image-upload.component.html',
  styleUrls: ['./product-image-upload.component.scss']
})
export class ProductImageUploadComponent {
  @Input() productId!: number;

  selectedFile: File | null = null;
  previewUrl: string | null = null;
  uploading = false;
  uploadedImageUrl: string | null = null;
  errorMessage: string | null = null;

  constructor(private http: HttpClient, private authService: AuthService) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.selectedFile = input.files[0];
      const reader = new FileReader();
      reader.onload = () => {
        this.previewUrl = reader.result as string;
      };
      reader.readAsDataURL(this.selectedFile);
    }
  }

  uploadImage(): void {
    if (!this.selectedFile || !this.productId) return;

    this.uploading = true;
    this.errorMessage = null;

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    const token = this.authService.getAccessToken();
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    this.http.post<{ imageUrl: string }>(
      `http://localhost:5213/api/products/${this.productId}/upload-image`,
      formData,
      { headers }
    ).subscribe({
      next: (res) => {
        this.uploadedImageUrl = `http://localhost:5213${res.imageUrl}`;
        this.uploading = false;
      },
      error: (err) => {
        this.errorMessage = 'Upload failed. Please try again.';
        this.uploading = false;
      }
    });
  }
}
