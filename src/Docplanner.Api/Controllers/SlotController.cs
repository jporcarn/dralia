using AutoMapper;
using Docplanner.Api.Models;
using Docplanner.Application.UseCases.Availability;
using Docplanner.Application.Utilities;
using Docplanner.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Docplanner.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SlotController : ControllerBase
    {
        private readonly IGetAvailableSlotsHandler _getAvailableSlotsHandler;
        private readonly IBookSlotHandler _bookSlotHandler;
        private readonly ILogger<SlotController> _logger;
        private readonly IMapper _mapper;

        public SlotController(
            IBookSlotHandler bookSlotHandler,
            IGetAvailableSlotsHandler getAvailableSlotsHandler,
            ILogger<SlotController> logger,
            IMapper mapper
            )
        {
            _bookSlotHandler = bookSlotHandler;
            _getAvailableSlotsHandler = getAvailableSlotsHandler;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get available slots for a given week and year.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="week"></param>
        /// <returns></returns>
        [HttpGet(Name = "GetWeeklySlots")]
        [ProducesResponseType(typeof(WeeklySlotsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WeeklySlotsResponse>> GetWeeklySlots([FromQuery] int year, [FromQuery] int week)
        {
            this._logger.LogInformation("GetWeeklySlots called with week: {week}", week);

            if (week is < 1 or > 53)
            {
                return BadRequest("Week number must be between 1 and 53.");
            }

            try
            {
                var response = await this._getAvailableSlotsHandler.GetWeeklySlotsAsync(year, week);
                if (response == null)
                {
                    return NotFound("No slots found for the given week.");
                }

                var apiResponse = this._mapper.Map<WeeklySlotsResponse>(response);

                return Ok(apiResponse);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Book a slot for a patient.
        /// </summary>
        /// <param name="startDate">A valid ISO 8601 representation of a DateTime in UTC</param>
        /// <param name="bookSlotRequest"></param>
        /// <returns></returns>
        [HttpPut("{startDate}/book", Name = "BookSlot")] // PUT by design, as I understand daily slots are a virtual collection and we are updating a resource in that collection.
        // You could argue it could be a POST if there are no items in the daily slots collection and we are creating a new resource on that collection
        [ProducesResponseType(typeof(WeeklySlotsResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<WeeklySlotsResponse>> BookSlot([FromRoute] DateTime startDate, [FromBody] BookSlotRequest bookSlotRequest)
        {
            this._logger.LogInformation("BookSlot called with startDate: {startDate}", startDate);

            if (startDate != bookSlotRequest.Start)
            {
                return BadRequest("Start date in the URL does not match the start date in the request body.");
            }

            var bookSlot = this._mapper.Map<BookSlot>(bookSlotRequest);

            await this._bookSlotHandler.BookSlotAsync(bookSlot);

            var response = await this._getAvailableSlotsHandler.GetWeeklySlotsAsync(bookSlotRequest.Start.Year, DateUtilities.GetWeekOfYear(bookSlotRequest.Start));
            if (response == null)
            {
                return NotFound("No slots found for the given week.");
            }

            return CreatedAtAction(
                nameof(GetWeeklySlots),
                new { year = bookSlotRequest.Start.Year, week = DateUtilities.GetWeekOfYear(bookSlotRequest.Start) },
                response
            );
        }
    }
}