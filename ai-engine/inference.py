from transformers import PegasusForConditionalGeneration, PegasusTokenizer
import torch

MODEL_NAME = "google/pegasus-xsum"

print("Loading Pegasus model...")

tokenizer = PegasusTokenizer.from_pretrained(MODEL_NAME)
model = PegasusForConditionalGeneration.from_pretrained(MODEL_NAME)
model.eval()

# Use GPU if available
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
model = model.to(device)

print(f"🚀 Model loaded on {device}")

def summarize_transformer(text: str, max_length: int = 128, min_length: int = 20):
    """
    Abstractive summarization using Pegasus.
    """
    inputs = tokenizer(
        text,
        truncation=True,
        padding="longest",
        return_tensors="pt",
        max_length=1024
    )

    input_ids = inputs["input_ids"].to(device)
    attention_mask = inputs["attention_mask"].to(device)

    summary_ids = model.generate(
        input_ids,
        attention_mask=attention_mask,
        max_length=max_length,
        min_length=min_length,
        num_beams=5,
        length_penalty=1.0,
        early_stopping=True,
    )

    summary = tokenizer.decode(summary_ids[0], skip_special_tokens=True)
    return summary
