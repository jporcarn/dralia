using Docplanner.Domain.Models;
using Docplanner.Infrastructure.SlotService.Models;

namespace Docplanner.Application.Interfaces.Repositories
{
    public interface IAvailabilityRepository
    {
        Task<WeeklyAvailabilityDto?> GetWeeklyAvailabilityAsync(DateOnly mondayDate);

        Task TakeSlotAsync(TakeSlotDto takeSlotDto);
    }
}