using Core.Entities;

namespace Application.Interfaces;

public interface IAIJobStore
{
    void Add(AIJob job);
    AIJob? Get(Guid id);
    void Update(AIJob job);
}