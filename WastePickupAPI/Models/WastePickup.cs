
namespace WastePickupAPI
{
    /// <summary>
    /// Provides the date and type of waste pickup
    /// </summary>
    public class WastePickup
    {
        /// <summary>
        /// Describes the date and type of waste pickup
        /// </summary>
        /// <param name="date"></param>
        /// <param name="pickupType"></param>
        public WastePickup(DateTime date, EPickupType pickupType)
        {
            Date = date;
            PickupType = pickupType;
        }

        /// <summary>
        /// The date of waste pickup
        /// </summary>
        public DateTime Date { get;init;}

        /// <summary>
        /// The type of waste pickup
        /// </summary>
        public EPickupType PickupType { get;init;}
    }
}
