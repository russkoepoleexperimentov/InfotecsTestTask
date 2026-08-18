namespace Application.Dtos
{
    public class ResultSearchDto
    {
        public string? NameQuery { get; set; }
        public RangeDto<DateTime>? FirstExecutionRange { get; set; }
        public RangeDto<float>? AverageValueRange { get; set; }
        public RangeDto<int>? AverageExcecutionTimeRange { get; set; }

        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;
    }
}
