using Core.Entities;

namespace Application.Interfaces;

public interface IAIQueue
{
    void Enqueue(AIJob job);
    AIJob? Dequeue();
}