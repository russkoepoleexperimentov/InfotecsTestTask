namespace Domain.Entities
{
    public class ValueRecord : BaseEntity
    {
        public DateTime Date { get; set; }
        public int ExceutionTime {  get; set; }
        public float Value { get; set; }

        public Guid FileResultId { get; set; }
        public FileResult FileResult { get; set; } = null!;
    }
}
