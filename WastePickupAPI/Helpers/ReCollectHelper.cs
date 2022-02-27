using Microsoft.AspNetCore.WebUtilities;

namespace WastePickupAPI
{
    public static class ReCollectHelper
    {
                #region ReCollectFunctions

        /// <summary>
        /// Gets a place object with a unique id known to ReCollect at the provided address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        /// 
        public async static Task<Models.ReCollect.Place?> GetValidAddress(string address, HttpClient httpClient)
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
                return  (await HttpClientHelper.GetAsync<Models.ReCollect.Place[]>(httpClient,uri))?.FirstOrDefault();
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
        public static async Task<WastePickup?> GetFirstPickup(Models.ReCollect.Place place, DateTime fromDate, DateTime toDate, EPickupType pickupType, HttpClient httpClient)
        {
            var queryParameters = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("after", fromDate.ToString("yyyy-MM-dd")),
                //have to add a day because it is exclusive in the reCollect api
                new KeyValuePair<string, string>("before", toDate.AddDays(1).ToString("yyyy-MM-dd")),
                new KeyValuePair<string, string>("locale","en-US")
            };

            var uri = QueryHelpers.AddQueryString($"https://api.recollect.net/api/places/{place.Id.ToString().ToUpper()}/services/842/events", queryParameters);

            var results = await HttpClientHelper.GetAsync<Models.ReCollect.CalendarResults>(httpClient,uri);

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
        private static string _getReCollectFlagName(EPickupType pickupType)
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

    }
}
