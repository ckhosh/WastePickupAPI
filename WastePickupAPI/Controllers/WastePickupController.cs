using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WastePickupAPI.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class WastePickupController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public WastePickupController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Gets the next waste pickup at the address of the specified type within the date range provided
        /// </summary>
        /// <param name="address" example="1422 Vance">Address</param>
        /// <param name="fromDate" example="1/1/2022">Start Date (Inclusive)</param>
        /// <param name="toDate" example="3/1/2022">End Date (Inclusive)</param>
        /// <param name="pickupType" example="Bulk">Pickup Type</param>
        /// <returns></returns>

        [HttpGet("[action]")]
        public async Task<ActionResult<WastePickup>> NextPickup(string address, DateTime fromDate, DateTime toDate, EPickupType pickupType)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var place = await ReCollectHelper.GetValidAddress(address, httpClient);
            if (place is null) return NotFound("Address not found");

            var pickup = await ReCollectHelper.GetFirstPickup(place, fromDate, toDate, pickupType, httpClient);

            return (pickup is not null ? Ok(pickup) : NotFound());
        }

        /// <summary>
        /// Gets the next scheduled pickup at the address of the specified type within the next four weeks
        /// </summary>
        /// <param name="address" example="1422 Vance">Address</param>
        /// <param name="pickupType" example="Garbage">Pickup Type</param>
        /// <returns></returns>
        [HttpGet("Next{pickupType}Pickup")]
        public async Task<ActionResult<WastePickup>> NextPickup(string address, EPickupType pickupType)
        {
           
            var fromDate = DateTime.Now.Date;
            var toDate = fromDate.AddDays(28);

            return await NextPickup(address,fromDate, toDate, pickupType);
        }

        /// <summary>
        /// Determines if the next pickup at the address of the specified type and within the specified date range is scheduled.
        /// </summary>
        /// <param name="address" example="1422 Vance">Address</param>
        /// <param name="fromDate" example="1/1/2022">Start Date (Inclusive)</param>
        /// <param name="toDate" example="3/1/2022">End Date (Inclusive)</param>
        /// <param name="pickupType" example="Bulk">Pickup Type</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<ActionResult<bool>> IsNextPickup(string address, DateTime fromDate, DateTime toDate, EPickupType pickupType)
        {
            //todo: call the previous function to determine if the pickup is within the specified time frame
            return Ok(true);
        }

        //todo: add more calls to simplify the IsNextPickup process for more natural langague, like is pickup next week, or next month, or this week.


    }
}