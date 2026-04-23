using BussinessObjects.DTOs.GameNews;
using BussinessObjects.Models;
using DataAccess.IRepositories;
using Services.IServices;

namespace Services.Services
{
    public class GameNewsService : IGameNewsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GameNewsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<List<GameNewsDto>> GetAll()
        {
            try
            {
                var newsList = _unitOfWork.GameNews.GetQueryable(asNoTracking: true).ToList();

                if (!newsList.Any())
                    return new ServiceResult<List<GameNewsDto>>
                    {
                        Success = true,
                        Message = "No game news found",
                        Data = new List<GameNewsDto>()
                    };

                var dtoList = newsList.Select(n => new GameNewsDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    BannerPath = n.BannerPath,
                    Description = n.Description,
                    Content = n.Content
                }).ToList();

                return new ServiceResult<List<GameNewsDto>>
                {
                    Success = true,
                    Message = "Game news retrieved successfully",
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<GameNewsDto>>
                {
                    Success = false,
                    Message = "Error retrieving game news",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<GameNewsDto>> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<GameNewsDto>
                    {
                        Success = false,
                        Message = "Invalid news ID"
                    };

                var news = await _unitOfWork.GameNews.FirstOrDefaultAsync(n => n.Id == id);

                if (news == null)
                    return new ServiceResult<GameNewsDto>
                    {
                        Success = false,
                        Message = "Game news not found"
                    };

                var dto = new GameNewsDto
                {
                    Id = news.Id,
                    Title = news.Title,
                    BannerPath = news.BannerPath,
                    Description = news.Description,
                    Content = news.Content
                };

                return new ServiceResult<GameNewsDto>
                {
                    Success = true,
                    Message = "Game news retrieved successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<GameNewsDto>
                {
                    Success = false,
                    Message = "Error retrieving game news",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<GameNewsDto>> CreateAsync(CreateGameNewsRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title) || 
                    string.IsNullOrWhiteSpace(request.BannerPath) ||
                    string.IsNullOrWhiteSpace(request.Description) ||
                    string.IsNullOrWhiteSpace(request.Content))
                {
                    return new ServiceResult<GameNewsDto>
                    {
                        Success = false,
                        Message = "All fields are required",
                        Errors = ["Title, BannerPath, Description, and Content cannot be empty"]
                    };
                }

                var existingNews = await _unitOfWork.GameNews.FirstOrDefaultAsync(n => n.Title == request.Title);
                if (existingNews != null)
                    return new ServiceResult<GameNewsDto>
                    {
                        Success = false,
                        Message = "A news with this title already exists"
                    };

                var news = new GameNews
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    BannerPath = request.BannerPath,
                    Description = request.Description,
                    Content = request.Content
                };

                await _unitOfWork.GameNews.AddAsync(news);
                await _unitOfWork.SaveChangesAsync();

                var dto = new GameNewsDto
                {
                    Id = news.Id,
                    Title = news.Title,
                    BannerPath = news.BannerPath,
                    Description = news.Description,
                    Content = news.Content
                };

                return new ServiceResult<GameNewsDto>
                {
                    Success = true,
                    Message = "Game news created successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<GameNewsDto>
                {
                    Success = false,
                    Message = "Error creating game news",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<GameNewsDto>> UpdateAsync(Guid id, UpdateGameNewsRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<GameNewsDto>
                    {
                        Success = false,
                        Message = "Invalid news ID"
                    };

                var news = await _unitOfWork.GameNews.FirstOrDefaultAsync(n => n.Id == id);
                if (news == null)
                    return new ServiceResult<GameNewsDto>
                    {
                        Success = false,
                        Message = "Game news not found"
                    };

                if (string.IsNullOrWhiteSpace(request.Title) ||
                    string.IsNullOrWhiteSpace(request.BannerPath) ||
                    string.IsNullOrWhiteSpace(request.Description) ||
                    string.IsNullOrWhiteSpace(request.Content))
                {
                    return new ServiceResult<GameNewsDto>
                    {
                        Success = false,
                        Message = "All fields are required",
                        Errors = ["Title, BannerPath, Description, and Content cannot be empty"]
                    };
                }

                // Check if title is changed and if new title already exists
                if (news.Title != request.Title)
                {
                    var existingNews = await _unitOfWork.GameNews.FirstOrDefaultAsync(n => n.Title == request.Title);
                    if (existingNews != null)
                        return new ServiceResult<GameNewsDto>
                        {
                            Success = false,
                            Message = "A news with this title already exists"
                        };
                }

                news.Title = request.Title;
                news.BannerPath = request.BannerPath;
                news.Description = request.Description;
                news.Content = request.Content;

                await _unitOfWork.GameNews.UpdateAsync(news);
                await _unitOfWork.SaveChangesAsync();

                var dto = new GameNewsDto
                {
                    Id = news.Id,
                    Title = news.Title,
                    BannerPath = news.BannerPath,
                    Description = news.Description,
                    Content = news.Content
                };

                return new ServiceResult<GameNewsDto>
                {
                    Success = true,
                    Message = "Game news updated successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<GameNewsDto>
                {
                    Success = false,
                    Message = "Error updating game news",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Invalid news ID"
                    };

                var news = await _unitOfWork.GameNews.FirstOrDefaultAsync(n => n.Id == id);
                if (news == null)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Game news not found"
                    };

                await _unitOfWork.GameNews.DeleteAsync(news);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = "Game news deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Error deleting game news",
                    Errors = [ex.Message]
                };
            }
        }
    }
}
