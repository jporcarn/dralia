import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SlotService } from 'src/app/services/slot.service';

@Component({
  selector: 'app-booking-form',
  templateUrl: './booking-form.component.html',
  styleUrls: ['./booking-form.component.css'],
})
export class BookingFormComponent implements OnInit {
  bookingForm!: FormGroup;
  start!: string;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private slotService: SlotService
  ) {}

  ngOnInit(): void {
    const startQueryParam = this.route.snapshot.paramMap.get('start')!;
    this.start = decodeURIComponent(startQueryParam);

    this.bookingForm = this.fb.group({
      comments: [''],
      patient: this.fb.group({
        name: ['', Validators.required],
        secondName: [''],
        email: ['', [Validators.required, Validators.email]],
        phone: ['', Validators.required],
      }),
    });
  }

  fillWithRandomData(): void {
    this.bookingForm.patchValue({
      comments: 'This is a test comment.',
      patient: {
        name: 'John',
        secondName: 'Doe',
        email: 'john.doe@example.com',
        phone: '1234567890',
      },
    });
  }

  onSubmit(): void {
    if (this.bookingForm.invalid) {
      // Mark all controls as touched to trigger validation messages
      this.bookingForm.markAllAsTouched();
      return;
    }

    const payload = {
      ...this.bookingForm.value,
      start: this.start,
    };

    const startDate = new Date(this.start);
    this.slotService.bookSlot(startDate, payload).subscribe({
      next: (response) => {
        console.log('Booking successful:', response);
        this.router.navigate(['/']);
      },
      error: (error) => {
        console.error('Error booking slot:', error);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/']);
  }
}
