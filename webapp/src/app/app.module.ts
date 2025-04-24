import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AvailabilityTableComponent } from './components/availability-table/availability-table.component';
import { BookingFormComponent } from './components/booking-form/booking-form.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { API_BASE_URL } from './services/slotapi-client.service';
import { environment } from '../environments/environment';

@NgModule({
  declarations: [
    AppComponent,
    AvailabilityTableComponent,
    BookingFormComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ReactiveFormsModule,
    HttpClientModule,
    FormsModule,
  ],
  providers: [
    { provide: API_BASE_URL, useValue: environment.apiBaseUrl }, // Provide the API base URL
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
