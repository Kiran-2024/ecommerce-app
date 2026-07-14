import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminRoleService, Role, Right } from '../../core/services/admin-role.service';

@Component({
  selector: 'app-admin-rights',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-rights.component.html',
  styleUrls: ['./admin-rights.component.css']
})
export class AdminRightsComponent implements OnInit {
  roles: Role[] = [];
  rights: Right[] = [];

  // matrix[roleId][rightId] = true/false
  matrix: { [roleId: number]: { [rightId: number]: boolean } } = {};

  loading = true;
  saving: { [roleId: number]: boolean } = {};

  constructor(private adminRoleService: AdminRoleService) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.adminRoleService.getAllRoles().subscribe({
      next: (roles) => {
        this.roles = roles;
        this.adminRoleService.getAllRights().subscribe({
          next: (rights) => {
            this.rights = rights;
            this.buildMatrixAndLoadAssignments();
          },
          error: () => (this.loading = false)
        });
      },
      error: () => (this.loading = false)
    });
  }

  buildMatrixAndLoadAssignments(): void {
    // ప్రతి role కి matrix row init చేయి, అన్నీ false
    this.roles.forEach(role => {
      this.matrix[role.roleId] = {};
      this.rights.forEach(right => {
        this.matrix[role.roleId][right.rightId] = false;
      });
    });

    // ప్రతి role కి already assigned rights fetch చేసి tick చేయి
    let completed = 0;
    this.roles.forEach(role => {
      this.adminRoleService.getRoleRights(role.roleId).subscribe({
        next: (assignedRights) => {
          assignedRights.forEach(r => {
            this.matrix[role.roleId][r.rightId] = true;
          });
          completed++;
          if (completed === this.roles.length) this.loading = false;
        },
        error: () => {
          completed++;
          if (completed === this.roles.length) this.loading = false;
        }
      });
    });
  }

  toggleRight(roleId: number, rightId: number): void {
    this.matrix[roleId][rightId] = !this.matrix[roleId][rightId];
  }

  saveRoleRights(roleId: number): void {
    const rightIds = this.rights
      .filter(r => this.matrix[roleId][r.rightId])
      .map(r => r.rightId);

    this.saving[roleId] = true;
    this.adminRoleService.assignRights(roleId, rightIds).subscribe({
      next: () => {
        this.saving[roleId] = false;
        alert('Rights updated successfully');
      },
      error: () => {
        this.saving[roleId] = false;
        alert('Failed to update rights');
      }
    });
  }
}