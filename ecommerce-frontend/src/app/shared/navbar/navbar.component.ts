import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { CartService } from '../../features/cart/cart.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {
  cartCount$!: Observable<number>;
  isLoggedIn = false;
  isAdminUser = false;

  constructor(
    private cartService: CartService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
    this.isAdminUser = this.authService.isAdmin();
    this.cartCount$ = this.cartService.cartCount$;
    if (this.isLoggedIn) {
      this.cartService.getCart().subscribe();
    }
  }

  logout(): void {
    const token = this.authService.getRefreshToken() || '';
    this.authService.logout(token).subscribe({
      next: () => {
        this.authService.clearTokens();
        this.isLoggedIn = false;
        this.router.navigate(['/login']);
      },
      error: () => {
        this.authService.clearTokens();
        this.isLoggedIn = false;
        this.router.navigate(['/login']);
      }
    });
  }
}