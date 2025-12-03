namespace MiniAppGIBA.Models.HomePins
{
    public class HomePinListResponse
    {
        public List<HomePinDto> Pins { get; set; } = new List<HomePinDto>();
        public int TotalCount { get; set; } 
        public int TotalPinsCount { get; set; }
        public int MaxPinsAllowed { get; set; }
        public bool CanAddMore { get; set; }
    }
}
