import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import * as moment from 'moment';
import { SlotService } from 'src/app/services/slot.service';
import { WeeklySlotsResponse } from 'src/app/services/slotapi-client.service';

@Component({
  selector: 'app-availability-table',
  templateUrl: './availability-table.component.html',
  styleUrls: ['./availability-table.component.css'],
  providers: [DatePipe],
})
export class AvailabilityTableComponent implements OnInit {
  weekdays = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'];

  slots: { [key: string]: string[] } = {
    Monday: ['9:00 AM', '10:00 AM', '11:00 AM', '12:00 PM', '1:00 PM'],
    Tuesday: ['9:00 AM', '10:00 AM', '11:00 AM', '12:00 PM', '1:00 PM'],
    Wednesday: ['9:00 AM', '10:00 AM', '11:00 AM', '12:00 PM', '1:00 PM'],
    Thursday: ['9:00 AM', '10:00 AM', '11:00 AM', '12:00 PM', '1:00 PM'],
    Friday: ['9:00 AM', '10:00 AM', '11:00 AM', '12:00 PM', '1:00 PM'],
  };

  years: number[] = [];
  weeks: {
    year: number;
    weekNumber: number;
    startDate: string;
    endDate: string;
  }[] = [];
  selectedWeek: any;
  selectedYear: number = moment().year();

  constructor(
    private router: Router,
    private slotService: SlotService,
    private datePipe: DatePipe
  ) {}

  ngOnInit(): void {
    this.generateYears();
    this.generateWeeks(this.selectedYear);
    this.selectedWeek = this.weeks[0]; // Default to the first week
    this.onWeekChange(new Event('init')); // Fetch slots for the default week
  }

  generateYears(): void {
    const currentYear = moment().year();
    this.years = [
      currentYear - 1,
      currentYear,
      currentYear + 1,
      currentYear + 2,
    ];
  }

  generateWeeks(year: number): void {
    this.weeks = []; // Reset weeks array
    const startOfYear = moment().year(year).startOf('year');
    const endOfYear = moment().year(year).endOf('year');
    let currentWeek = startOfYear.clone().startOf('isoWeek');

    while (currentWeek.isBefore(endOfYear)) {
      const weekNumber = currentWeek.isoWeek();
      const startDate = currentWeek.format('YYYY-MM-DD');
      const endDate = currentWeek.clone().endOf('isoWeek').format('YYYY-MM-DD');

      this.weeks.push({
        year,
        weekNumber,
        startDate,
        endDate,
      });

      currentWeek.add(1, 'week');
    }
  }

  onBook(event: Event, day: string, slot: string): void {
    event.preventDefault(); // Cancel the default click behavior

    console.log(`Booking ${slot} on ${day}`);
    this.router.navigate(['/book', slot]);
  }

  onWeekChange(event: Event): void {
    console.log('Selected year:', this.selectedYear);
    if (!this.selectedWeek) {
      console.error('No week selected');
      return;
    }

    console.log('Selected week:', this.selectedWeek);
    if (!this.selectedWeek) {
      console.error('No week selected');
      return;
    }

    // Call the API to fetch weekly slots
    this.slotService
      .getWeeklySlots(this.selectedYear, this.selectedWeek.weekNumber)
      .subscribe({
        next: (response: WeeklySlotsResponse) => {
          console.log('Weekly slots response:', response);
          this.updateSlots(response); // Update the slots table with the response
        },
        error: (error) => {
          console.error('Error fetching weekly slots:', error);
        },
      });
  }

  onYearChange(event: Event): void {
    this.generateWeeks(this.selectedYear); // Regenerate weeks for the selected year
    this.selectedWeek = this.weeks[0]; // Reset to the first week of the selected year
    this.onWeekChange(event); // Trigger week change logic
  }

  private updateSlots(response: WeeklySlotsResponse): void {
    // Reset slots
    this.slots = {};

    // Map each DailySlotsResponse to the slots object
    if (response.days) {
      response.days.forEach((dailySlot) => {
        if (dailySlot.dayOfWeek && dailySlot.slots) {
          // Map dayOfWeek to the key and extract start times from slots
          const slots = dailySlot.slots.map(
            (slot) => this.datePipe.transform(slot.start, 'HH:mm') ?? '??:??'
          );

          this.slots[dailySlot.dayOfWeek] = slots;
        }
      });
    }
    console.log('Mapped slots:', this.slots);
  }
}
