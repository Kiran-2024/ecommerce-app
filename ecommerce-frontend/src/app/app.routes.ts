import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { rightsGuard } from './core/guards/rights.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },

  {
    path: 'auth',
    loadChildren: () =>
      import('./auth/auth.module').then(m => m.AuthModule)
  },

  // HOME - home/home/home.component
  {
    path: 'home',
    loadComponent: () =>
      import('./home/home/home.component').then(m => m.HomeComponent),
    canActivate: [authGuard]
  },

   // PRODUCTS - features/products
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
        loadComponent: () =>
          import('./admin/products/product-list/product-list.component').then(m => m.ProductListComponent),
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