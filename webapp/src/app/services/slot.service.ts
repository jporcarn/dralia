import { Inject, Injectable } from '@angular/core';
import {
  BookSlotRequest,
  SlotApiClient,
  WeeklySlotsResponse,
} from './slotapi-client.service';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SlotService {
  constructor(@Inject(SlotApiClient) private slotApiClient: SlotApiClient) {}

  /**
   * Fetch weekly slots for a given year and week.
   * @param year The year for which to fetch slots.
   * @param week The week number for which to fetch slots.
   * @returns An Observable resolving to the weekly slots response.
   */
  getWeeklySlots(year: number, week: number): Observable<WeeklySlotsResponse> {
    return this.slotApiClient.getWeeklySlots(year, week).pipe(
      catchError((error) => {
        console.error('Error fetching weekly slots:', error);
        return throwError(() => error); // Re-throw the error
      })
    );
  }

  /**
   * Book a slot for a given start date and booking details.
   * @param startDate The start date of the slot to book.
   * @param bookingRequest The booking details.
   * @returns An Observable resolving to the booking response.
   */
  bookSlot(
    startDate: Date,
    bookingRequest: BookSlotRequest
  ): Observable<WeeklySlotsResponse> {
    return this.slotApiClient.bookSlot(startDate, bookingRequest).pipe(
      catchError((error) => {
        console.error('Error booking slot:', error);
        return throwError(() => error); // Re-throw the error
      })
    );
  }
}
