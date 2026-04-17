using BussinessObjects.DTOs.Gacha;
using BussinessObjects.Models;

namespace Services.IServices
{
    public interface IGachaService
    {
        // ── User actions ──────────────────────────────────
        Task<ServiceResult<GachaPullResponseDto>> SinglePullAsync(Guid userId, GachaSinglePullRequest request);
        Task<ServiceResult<GachaPullResponseDto>> MultiPullAsync(Guid userId, GachaMultiPullRequest request);
        Task<ServiceResult<UserGachaStatusDto>> GetUserGachaStatusAsync(Guid userId, Guid bannerId);
        Task<ServiceResult<List<GachaHistoryDto>>> GetUserHistoryAsync(Guid userId, int page = 1, int pageSize = 20);

        // ── Banner info (public) ──────────────────────────
        Task<ServiceResult<List<GachaBannerDto>>> GetActiveBannersAsync();
        Task<ServiceResult<GachaBannerDto>> GetBannerByIdAsync(Guid bannerId);

        // ── Admin ─────────────────────────────────────────
        Task<ServiceResult<GachaBannerDto>> CreateBannerAsync(CreateGachaBannerRequest request);
        Task<ServiceResult<GachaBannerDto>> UpdateBannerAsync(Guid bannerId, UpdateGachaBannerRequest request);
        Task<ServiceResult<bool>> ToggleBannerAsync(Guid bannerId);
        Task<ServiceResult<bool>> AddItemToBannerAsync(Guid bannerId, AddGachaItemRequest request);
        Task<ServiceResult<bool>> RemoveItemFromBannerAsync(Guid bannerId, Guid itemId);
    }
}

