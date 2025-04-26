import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppComponent } from './app.component';
import { AvailabilityTableComponent } from './components/availability-table/availability-table.component';
import { BookingFormComponent } from './components/booking-form/booking-form.component';

const routes: Routes = [
  { path: '', component: AppComponent },
  { path: 'slot', component: AvailabilityTableComponent },
  { path: 'book/:start', component: BookingFormComponent },
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
