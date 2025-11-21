namespace GAM106_LAB.Models
{
    public class ApiResponse<T>
    {
        public int Status { get; set; }
        public string? Message { get; set; } = null!;
        public T? Data { get; set; }
    }
}
