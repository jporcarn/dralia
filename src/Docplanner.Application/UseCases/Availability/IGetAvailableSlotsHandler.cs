using Docplanner.Domain.Models;

namespace Docplanner.Application.UseCases.Availability
{
    public interface IGetAvailableSlotsHandler
    {
        Task<WeeklySlots> GetWeeklySlotsAsync(int year, int week);
    }
}