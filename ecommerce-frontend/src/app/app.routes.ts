import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { rightsGuard } from './core/guards/rights.guard';
import { AdminProductsComponent } from './features/admin/products/admin-products.component';

export const routes: Routes = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },

  {
    path: 'auth',
    loadChildren: () =>
      import('./auth/auth.module').then(m => m.AuthModule)
  },

  {
    path: 'addresses',
    loadComponent: () =>
      import('./features/address/address-book/address-book.component').then(m => m.AddressBookComponent),
    canActivate: [authGuard]
  },

  {
    path: 'home',
    loadComponent: () =>
      import('./home/home/home.component').then(m => m.HomeComponent),
    canActivate: [authGuard]
  },

  {
    path: 'products',
    loadComponent: () =>
      import('./features/products/product-list/product-list.component').then(m => m.ProductListComponent),
    canActivate: [authGuard]
  },

  {
    path: 'products/:id',
    loadComponent: () =>
      import('./features/products/product-detail/product-detail.component').then(m => m.ProductDetailComponent),
    canActivate: [authGuard]
  },

  {
    path: 'cart',
    loadComponent: () =>
      import('./features/cart/cart/cart.component').then(m => m.CartComponent),
    canActivate: [authGuard]
  },

  {
    path: 'checkout',
    loadComponent: () =>
      import('./features/checkout/checkout/checkout.component').then(m => m.CheckoutComponent),
    canActivate: [authGuard]
  },

  {
    path: 'orders',
    loadComponent: () =>
      import('./orders/orders.component').then(m => m.OrdersComponent),
    canActivate: [authGuard]
  },

  {
    path: 'orders/:id',
    loadComponent: () =>
      import('./features/orders/order-detail/order-detail.component').then(m => m.OrderDetailComponent),
    canActivate: [authGuard]
  },

  {
    path: 'admin',
    canActivate: [adminGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./admin/dashboard/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'products',
        component: AdminProductsComponent,
        canActivate: [rightsGuard],
        data: { requiredRight: 'product.view' }
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./admin/users/user-list/user-list.component').then(m => m.UserListComponent),
        canActivate: [rightsGuard],
        data: { requiredRight: 'user.view' }
      },
      {
        path: 'orders',
        loadComponent: () =>
          import('./admin/orders/order-list/order-list.component').then(m => m.OrderListComponent),
        canActivate: [rightsGuard],
        data: { requiredRight: 'order.view' }
      }
    ]
  },

  {
    path: 'unauthorized',
    loadComponent: () =>
      import('./shared/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
  },

  { path: '**', redirectTo: 'auth/login' }
];