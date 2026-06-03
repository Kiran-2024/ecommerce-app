import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  template: `
    <div style="text-align:center; margin-top: 100px;">
      <h1 style="color: #e53e3e; font-size: 48px;">🚫 403</h1>
      <h2 style="color: #2d3748;">Access Denied</h2>
      <p style="color: #718096;">You do not have permission to view this page.</p>
      <button (click)="goBack()"
        style="margin-top:20px; padding: 10px 24px; background:#3182ce;
               color:white; border:none; border-radius:6px; cursor:pointer; font-size:16px;">
        ← Go Back
      </button>
    </div>
  `
})
export class UnauthorizedComponent {
  constructor(private router: Router) {}

  goBack(): void {
    this.router.navigate(['/home']);
  }
}