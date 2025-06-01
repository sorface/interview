namespace Interview.Domain.Rooms.BusinessAnalytic;

public class BusinessAnalyticResponse
{
    public required List<Item> Ai { get; set; }

    public required List<Item> Standard { get; set; }

    public class Item
    {
        public EVRoomAccessType AccessType { get; set; }

        public required DateOnly Date { get; set; }

        public required List<ItemStatus> Status { get; set; }
    }

    public class ItemStatus
    {
        public required EVRoomStatus Name { get; set; }

        public required int Count { get; set; }
    }
}
