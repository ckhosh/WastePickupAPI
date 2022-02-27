using System.Text.Json.Serialization;

namespace WastePickupAPI.Models.ReCollect
{
    public class Place
    {
            [JsonPropertyName("place_id")]
            public Guid Id { get; init; }
    }
}
