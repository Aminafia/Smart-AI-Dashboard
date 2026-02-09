namespace Core.AI.Models;

public class AIRequest
{
    // Business intent
    public string InputText { get; set; } = string.Empty;
    public string TaskType { get; set; } = "analysis";

    // LLM controls
    public int MaxTokens { get; set; } = 500; //prevents runway cosr
    public double Temperature { get; set; } = 0.7; //controls creativity (0.0=deterministic, 0.7=creative (default), >1.0=risky chaos)
}