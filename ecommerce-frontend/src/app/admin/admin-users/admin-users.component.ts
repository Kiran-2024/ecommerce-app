import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminUserService, AdminUser, AdminRole } from '../../core/services/admin-user.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-users.component.html',
  styleUrls: ['./admin-users.component.css']
})
export class AdminUsersComponent implements OnInit {
  users: AdminUser[] = [];
  roles: AdminRole[] = [];
  totalCount = 0;
  totalPages = 0;
  currentPage = 1;
  pageSize = 10;

  // Filters
  searchText = '';
  selectedRole = '';

  loading = false;

  constructor(
    private adminUserService: AdminUserService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadRoles();
    this.loadUsers();
  }

  loadRoles(): void {
    this.adminUserService.getRoles().subscribe({
      next: (res) => (this.roles = res),
      error: () => this.toastr.error('Roles load cheyyadam fail ayindi')
    });
  }

  loadUsers(): void {
    this.loading = true;
    this.adminUserService.getUsers({
      page: this.currentPage,
      pageSize: this.pageSize,
      search: this.searchText || undefined,
      role: this.selectedRole || undefined
    }).subscribe({
      next: (res) => {
        this.users = res.users;
        this.totalCount = res.totalCount;
        this.totalPages = res.totalPages;
        this.loading = false;
      },
      error: () => {
        this.toastr.error('Users load cheyyadam fail ayindi');
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  resetFilters(): void {
    this.searchText = '';
    this.selectedRole = '';
    this.currentPage = 1;
    this.loadUsers();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadUsers();
  }

  toggleStatus(user: AdminUser): void {
    const newStatus = !user.isActive;
    this.adminUserService.updateStatus(user.userId, newStatus).subscribe({
      next: () => {
        user.isActive = newStatus;
        this.toastr.success(`${user.fullName} ${newStatus ? 'activated' : 'deactivated'}`);
      },
      error: () => this.toastr.error('Status update fail ayindi')
    });
  }

  changeRole(user: AdminUser, newRoleName: string): void {
    if (user.role === newRoleName) return;

    const role = this.roles.find(r => r.roleName === newRoleName);
    if (!role) return;

    this.adminUserService.updateRole(user.userId, role.roleId).subscribe({
      next: () => {
        user.role = newRoleName;
        this.toastr.success(`${user.fullName} role updated to ${newRoleName}`);
      },
      error: () => this.toastr.error('Role update fail ayindi')
    });
  }

  getPages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }
}