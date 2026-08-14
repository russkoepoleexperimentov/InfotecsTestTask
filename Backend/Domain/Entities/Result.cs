namespace Domain.Entities
{
    public class Result : BaseEntity
    {
        public string FileName { get; set; } = null!;
        public int DeltaSeconds { get; set; }
        public DateTime FirstExcecutionTime { get; set; }
        public int AverageExcecutionTime { get; set; }
        public float AverageValue { get; set; }
        public float MedianValue { get; set; }
        public float MinimumValue { get; set; }
        public float MaximumValue { get; set; }
    }
}
