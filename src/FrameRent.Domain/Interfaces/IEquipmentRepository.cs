using FrameRent.Domain.Entities;

namespace FrameRent.Domain.Interfaces;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Equipment>> GetActiveAsync(CancellationToken ct = default);
    Task<List<Equipment>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Equipment equipment, CancellationToken ct = default);
    void Update(Equipment equipment);
}
