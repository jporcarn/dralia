using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Domain.Models;
using Docplanner.Infrastructure.SlotService.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Docplanner.Application.UseCases.Availability
{
    public class BookSlotHandler : IBookSlotHandler
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly ILogger<BookSlotHandler> _logger;

        public BookSlotHandler(
            IAvailabilityRepository availabilityRepository,
            ILogger<BookSlotHandler> logger)
        {
            _availabilityRepository = availabilityRepository;
            _logger = logger;
        }

        public async Task BookSlotAsync(BookSlot bookSlot)
        {
            _logger.LogInformation("BookSlotAsync called with slot start date: {startDate}", bookSlot.Start);

            // To ensure these calculations are consittent, we should use the same logic in the API controller
            // and in the handler.
            // Different strategies for calculating the week number can lead to different results.
            // TODO: Implement different strategies using strategy pattern for calculating the week number
            // For example, the ISO-8601 standard defines a week as starting on Monday and the first week of the year
            // Other strategies define a week as starting on Sunday and the first week of the year as the week containing January 1st.
            var year = bookSlot.Start.Year;
            var week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(bookSlot.Start, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            var mondayDateOnly = Utilities.DateUtilities.GetMondayOfGivenYearAndWeek(year, week);

            var availableSlots = await this._availabilityRepository.GetWeeklyAvailabilityAsync(mondayDateOnly);

            var availableSlot = availableSlots.Days.SelectMany(d => d.Slots)
                .FirstOrDefault(s => s.Start == bookSlot.Start);

            if (availableSlot == null)
            {
                throw new KeyNotFoundException("No available slot found for the given start date.");
            }

            var end = bookSlot.Start.AddMinutes(availableSlots.SlotDurationMinutes);

            // Map domain model to DTO
            // The Application project should handle the mapping of BookSlot to TakeSlotDto because it involves domain logic.
            // The Application project should handle mapping only if domain logic is required to construct the DTO.
            var takeSlotDto = new TakeSlotDto
            {
                FacilityId = availableSlots.Facility.FacilityId,
                Start = bookSlot.Start,
                End = end,
                Comments = bookSlot.Comments,
                Patient = new PatientDto
                {
                    Name = bookSlot.Patient.Name,
                    SecondName = bookSlot.Patient.SecondName,
                    Email = bookSlot.Patient.Email,
                    Phone = bookSlot.Patient.Phone
                }
            };

            await this._availabilityRepository.TakeSlotAsync(takeSlotDto);

        }
    }
}