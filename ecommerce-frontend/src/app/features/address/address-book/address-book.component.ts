import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AddressService, Address, CreateAddress } from '../address.service';

@Component({
  selector: 'app-address-book',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './address-book.component.html',
  styleUrls: ['./address-book.component.scss']
})
export class AddressBookComponent implements OnInit {
  addresses: Address[] = [];
  showForm = false;
  editingId: number | null = null;
  loading = false;
  addressForm!: FormGroup;

  constructor(private addressService: AddressService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initForm();
    this.loadAddresses();
  }

  initForm(): void {
    this.addressForm = this.fb.group({
      fullName: ['', Validators.required],
      phoneNumber: ['', Validators.required],
      addressLine1: ['', Validators.required],
      addressLine2: [''],
      city: ['', Validators.required],
      state: ['', Validators.required],
      pinCode: ['', Validators.required],
      isDefault: [false]
    });
  }

  loadAddresses(): void {
    this.loading = true;
    this.addressService.getAddresses().subscribe({
      next: (data) => {
        this.addresses = data;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  openAddForm(): void {
    this.editingId = null;
    this.addressForm.reset({ isDefault: false });
    this.showForm = true;
  }

  openEditForm(address: Address): void {
    this.editingId = address.addressId;
    this.addressForm.patchValue(address);
    this.showForm = true;
  }

  saveAddress(): void {
    if (this.addressForm.invalid) return;
    const dto: CreateAddress = this.addressForm.value;
    if (this.editingId) {
      this.addressService.updateAddress(this.editingId, dto).subscribe({
        next: () => { this.showForm = false; this.loadAddresses(); }
      });
    } else {
      this.addressService.createAddress(dto).subscribe({
        next: () => { this.showForm = false; this.loadAddresses(); }
      });
    }
  }

  deleteAddress(id: number): void {
    if (!confirm('Delete this address?')) return;
    this.addressService.deleteAddress(id).subscribe({
      next: () => this.loadAddresses()
    });
  }

  setDefault(id: number): void {
    this.addressService.setDefault(id).subscribe({
      next: () => this.loadAddresses()
    });
  }

  cancel(): void {
    this.showForm = false;
    this.addressForm.reset();
  }
}