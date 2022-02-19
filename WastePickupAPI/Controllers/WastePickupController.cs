using Microsoft.AspNetCore.Mvc;
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
            var place = await _getValidAddress(address, httpClient);
            if (place is null) return NotFound("Address not found");

            var pickup = await _getFirstPickup(place, fromDate, toDate, pickupType, httpClient);

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


        #region ReCollectFunctions

        private async Task<ReCollectPlace?> _getValidAddress(string address, HttpClient httpClient)
        {
            //todo: need to determine if the given address is valid for , should return the guid associated with th e

            var queryParameters = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("q", address),
                new KeyValuePair<string, string>("locale","en-US")
            };

            var uri = QueryHelpers.AddQueryString("https://api.recollect.net/api/areas/MemphisTN/services/842/address-suggest",queryParameters);

            try
            {            
                return  (await HttpClientHelper.GetAsync<ReCollectPlace[]>(httpClient,uri))?.FirstOrDefault();
            }
            catch
            {
                return null; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="place"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="pickupType"></param>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        private async Task<WastePickup?> _getFirstPickup(ReCollectPlace place, DateTime fromDate, DateTime toDate, EPickupType pickupType, HttpClient httpClient)
        {
            var queryParameters = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("after", fromDate.ToString("yyyy-MM-dd")),
                //have to add a day because it is exclusive in the reCollect api
                new KeyValuePair<string, string>("before", toDate.AddDays(1).ToString("yyyy-MM-dd")),
                new KeyValuePair<string, string>("locale","en-US")
            };

            var uri = QueryHelpers.AddQueryString($"https://api.recollect.net/api/places/{place.Id.ToString().ToUpper()}/services/842/events", queryParameters);

            var results = await HttpClientHelper.GetAsync<ReCollectCalendarResults>(httpClient,uri);

            if (results == null) return null;

            var reCollectFlagName = _getReCollectFlagName(pickupType);

            var reEvent = results.Events.FirstOrDefault(evnt=> evnt.Flags.Any(flag=> flag.Name.Equals(reCollectFlagName,StringComparison.InvariantCultureIgnoreCase)));

            if (reEvent is null) return null;

            return new WastePickup(reEvent.Day, pickupType);

        }



        /// <summary>
        /// Converts the our pickup type to the string equivalent in ReCollect
        /// </summary>
        /// <param name="pickupType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string _getReCollectFlagName(EPickupType pickupType)
        {
            switch (pickupType)
            {

                case EPickupType.Garbage:
                    return "garbage";
                case EPickupType.Bulk:
                    return "noncartedwaste";
                case EPickupType.YardWaste:
                    return "yardwaste";
                case EPickupType.Recyling:
                    return "recycling";
                default:
                    throw new ArgumentOutOfRangeException(nameof(pickupType));

            }
        }

        #endregion

        #region ReCollectObjects

        internal record ReCollectPlace
        {
            [JsonPropertyName("place_id")]
            public Guid Id { get; set; }
        }

        internal record ReCollectCalendarResults
        {
            internal record ReCollectEvent
            {
                internal record ReCollectFlag
                {
                    public string Name { get; set; }
                }
                public DateTime Day { get; set; }

                public ReCollectFlag[] Flags { get; set; }

            }
            public ReCollectEvent[] Events { get; set; }

        }
        #endregion
    }
}