import re

def split_sentences(text: str):
    """
    Lightweight sentence splitter for extractive summarization.
    """
    text = text.strip()
    sentences = re.split(r'(?<=[.!?])\s+', text)
    return [s.strip() for s in sentences if s.strip()]
