using Docplanner.Api.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Docplanner.Application.Interfaces.Repositories
{
    public interface IAvailabilityRepository
    {
        Task<WeeklySlots> GetWeeklyAvailabilityAsync(DateOnly mondayDate);
    }
}