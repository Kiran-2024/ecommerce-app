import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-otp-verify',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './otp-verify.component.html',
  styleUrl: './otp-verify.component.scss'
})
export class OtpVerifyComponent implements OnInit {
  form: FormGroup;
  errorMessage = '';
  successMessage = '';
  loading = false;
  resendLoading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      otp: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
    });
  }

  ngOnInit() {
    const email = this.route.snapshot.queryParams['email'];
    if (email) {
      this.form.patchValue({ email });
    }
  }

  onVerify() {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';
    this.authService.verifyOtp(this.form.value).subscribe({
      next: () => {
        this.router.navigate(['/auth/login']);
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'OTP verification failed!';
        this.loading = false;
      }
    });
  }

  onResend() {
    const email = this.form.value.email;
    if (!email) return;
    this.resendLoading = true;
    this.authService.resendOtp(email).subscribe({
      next: () => {
        this.successMessage = 'OTP resent successfully!';
        this.resendLoading = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Resend failed!';
        this.resendLoading = false;
      }
    });
  }
}
