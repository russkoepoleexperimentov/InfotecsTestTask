namespace Domain.Entities
{
    public class ValueRecord : BaseEntity
    {
        public DateTime Date { get; set; }
        public int ExceutionTime {  get; set; }
        public float Value { get; set; }
    }
}
