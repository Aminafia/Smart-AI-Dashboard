namespace Core.Entities
{
    public class AResult
    {
        public Guid Id { get; set; }
        public Guid AIJobId { get; set; } = default!;
        public string OutputJson { get; set; } = default!;
        public string ModelUsed { get; set; } = default!;
        public int TokenUsed { get; set; }
    }
}