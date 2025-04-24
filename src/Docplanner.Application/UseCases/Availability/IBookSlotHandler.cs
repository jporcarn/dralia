using Docplanner.Domain.Models;

namespace Docplanner.Application.UseCases.Availability
{
    public interface IBookSlotHandler
    {
        Task BookSlotAsync(BookSlot bookSlotRequest);
    }
}