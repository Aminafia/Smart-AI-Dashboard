from fastapi import FastAPI
from pydantic import BaseModel
from fastapi.middleware.cors import CORSMiddleware

from inference import summarize_transformer

app = FastAPI(title="AI Engine - Transformer Summarizer")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # dev only
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class SummarizeRequest(BaseModel):
    text: str
    max_length: int = 128
    min_length: int = 20

@app.post("/summarize")
def summarize(req: SummarizeRequest):
    summary = summarize_transformer(
        req.text,
        max_length=req.max_length,
        min_length=req.min_length
    )
    return {"summary": summary}

@app.get("/")
def health():
    return {"status": "ok", "model": "pegasus-xsum"}
