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

    public class GameNewsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string BannerPath { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }

    public class CreateGameNewsRequest
    {
        public string Title { get; set; }
        public string BannerPath { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }

    public class UpdateGameNewsRequest
    {
        public string Title { get; set; }
        public string BannerPath { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }
}
