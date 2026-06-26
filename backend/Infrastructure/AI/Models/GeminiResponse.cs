public class GeminiResponse
{
    public List<GeminiCandidate> Candidates { get; set; } = new();
}

public class GeminiCandidate
{
    public GeminiContentResponse Content { get; set; } = new();
}

public class GeminiContentResponse
{
    public List<GeminiPartResponse> Parts { get; set; } = new();
}

public class GeminiPartResponse
{
    public string Text { get; set; } = string.Empty;
}