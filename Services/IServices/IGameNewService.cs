using BussinessObjects.DTOs.GameNews;

namespace Services.IServices
{
    public interface IGameNewsService
    {
        ServiceResult<List<GameNewsDto>> GetAll();
        Task<ServiceResult<GameNewsDto>> GetByIdAsync(Guid id);
        Task<ServiceResult<GameNewsDto>> CreateAsync(CreateGameNewsRequest request);
        Task<ServiceResult<GameNewsDto>> UpdateAsync(Guid id, UpdateGameNewsRequest request);
        Task<ServiceResult<bool>> DeleteAsync(Guid id);
    }
}
