import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent {
  form: FormGroup;
  errorMessage = '';
  successMessage = '';
  loading = false;

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.authService.forgotPassword(this.form.value.email).subscribe({
      next: () => {
        this.router.navigate(['/auth/reset-password'], {
          queryParams: { email: this.form.value.email }
        });
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to send OTP!';
        this.loading = false;
      }
    });
  }
}
