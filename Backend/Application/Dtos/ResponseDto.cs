namespace Application.Dtos
{
    public class ResponseDto
    {
        public string? Details { get; set; }
    }

    public class ResponseDto<TResponse> : ResponseDto
    {
        public TResponse? Data { get; set; }
    }
}
