using Docplanner.Api.Models;

namespace Docplanner.Application.Interfaces.Repositories
{
    public interface IAvailabilityRepository
    {
        Task<WeeklySlots> GetWeeklyAvailabilityAsync(DateOnly mondayDate);
    }
}