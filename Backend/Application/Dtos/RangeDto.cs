namespace Application.Dtos
{
    // struct нужен, чтобы незаполненная граница не превращалась в 0
    public class RangeDto<T> where T : struct
    {
        public T? Min { get; set; }
        public T? Max { get; set; }
    }
}
