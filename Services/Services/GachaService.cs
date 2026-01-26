using BussinessObjects.Models;
using DataAccess.IRepositories;
using Services.IServices;

namespace Services.Services
{
    public class GachaService : IGachaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Random _random;

        public GachaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _random = new Random();
        }

        public Task<List<UserItem>> RollMultiplesAsync(Guid userId, int count)
        {
            throw new NotImplementedException();
        }

        public Task<UserItem> RollSingleAsync(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}

