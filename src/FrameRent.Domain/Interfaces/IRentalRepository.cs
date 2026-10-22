using FrameRent.Domain.Entities;
using FrameRent.Domain.Enums;

namespace FrameRent.Domain.Interfaces;

public interface IRentalRepository
{
    Task<Rental?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Rental?> GetByIdWithEquipmentAsync(Guid id, CancellationToken ct = default);
    Task<List<Rental>> GetByClientIdAsync(string clientId, CancellationToken ct = default);
    Task<List<Rental>> GetAllAsync(RentalStatus? status, CancellationToken ct = default);
    Task<List<Rental>> GetBlockingRentalsForEquipmentAsync(Guid equipmentId, CancellationToken ct = default);
    Task<bool> HasActiveCommitmentsAsync(Guid equipmentId, CancellationToken ct = default);
    Task<int> CountActiveForClientAsync(string clientId, CancellationToken ct = default);
    Task<List<(Guid EquipmentId, int RentalCount)>> GetMostRentedEquipmentAsync(CancellationToken ct = default);
    Task AddAsync(Rental rental, CancellationToken ct = default);
    void Update(Rental rental);
}
