namespace Application.Dtos
{
    public class PagedListDto<T>
    {
        public List<T> Values { get; set; } = null!;
        public int Skipped { get; set; }
        public int Taken { get; set; }
        public int TotalCount { get; set; }
    }
}
