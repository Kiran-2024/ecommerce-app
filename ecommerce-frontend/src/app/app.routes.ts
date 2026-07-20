import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { rightsGuard } from './core/guards/rights.guard';
import { AdminProductsComponent } from './features/admin/products/admin-products.component';
import { AdminUsersComponent } from './admin/admin-users/admin-users.component';

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
        component: AdminUsersComponent,
        canActivate: [authGuard, rightsGuard],
        data: { requiredRight: 'user.manage' }
      },
      {
        path: 'orders',
        loadComponent: () =>
          import('./admin/admin-orders/admin-orders.component').then(m => m.AdminOrdersComponent),
        canActivate: [rightsGuard],
        data: { requiredRight: 'order.manage' }
      },
      {
        path: 'rights',
        loadComponent: () =>
          import('./admin/admin-rights/admin-rights.component').then(m => m.AdminRightsComponent),
        canActivate: [rightsGuard],
        data: { requiredRight: 'role.manage' }
      },
      {
        path: 'categories',
        loadComponent: () =>
          import('./admin/admin-categories/admin-categories.component').then(m => m.AdminCategoriesComponent),
        canActivate: [rightsGuard],
        data: { requiredRight: 'category.edit' }
      },
      {
        path: 'reports',
        loadComponent: () => import('./admin/admin-reports/admin-reports.component').then(m => m.AdminReportsComponent),
        canActivate: [authGuard, rightsGuard],
        data: { requiredRight: 'reports.view' }
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