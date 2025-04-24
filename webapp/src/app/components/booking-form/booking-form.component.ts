import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

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
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.start = this.route.snapshot.paramMap.get('start')!;
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

  onSubmit(): void {
    if (this.bookingForm.valid) {
      const payload = {
        ...this.bookingForm.value,
        start: this.start,
      };

      this.http
        .put(`https://localhost:7236/slot/${this.start}/book`, payload)
        .subscribe({
          next: () => this.router.navigate(['/']),
          error: (err) => alert('Failed to book slot: ' + err.message),
        });
    }
  }

  onCancel(): void {
    this.router.navigate(['/']);
  }
}
