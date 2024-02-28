using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RestaurantAPI.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StatusEnum
    {
        Ready = 1,
        Working = 2,
        ReadyToPay = 3,
        Paid = 4
    }
}
