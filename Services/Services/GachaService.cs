using BussinessObjects.DTOs.Gacha;
using BussinessObjects.Models;
using DataAccess.IRepositories;
using Microsoft.EntityFrameworkCore;
using Services.IServices;

namespace Services.Services
{
    public class GachaService : IGachaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Random _rng = new();

        public GachaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ════════════════════════════════════════════════════
        // SINGLE PULL  —  Wish x1  (100 Gems)
        // ════════════════════════════════════════════════════
        public async Task<ServiceResult<GachaPullResponseDto>> SinglePullAsync(
            Guid userId, GachaSinglePullRequest request)
        {
            try
            {
                var (user, banner, gachaItems, error) = await ValidatePullAsync(userId, request.BannerId);
                if (error != null) return Fail<GachaPullResponseDto>(error);

                if (user!.CurrencyAmount < banner!.CostPerSinglePull)
                    return Fail<GachaPullResponseDto>(
                        $"Not enough Gems. Required: {banner.CostPerSinglePull}, Available: {user.CurrencyAmount}");

                // Lấy existing inventory để check IsNew
                var ownedItemIds = GetOwnedItemIds(userId);

                // Thực hiện 1 pull
                user.PityCounter++;
                var (pulledGachaItem, wasPity) = RollSingleItem(gachaItems!, user.PityCounter, banner.HardPityThreshold);
                if (wasPity) user.PityCounter = 0;

                var result = BuildResult(pulledGachaItem, wasPity, ownedItemIds, pullNumber: 1);

                // Cập nhật inventory + lịch sử
                await AddToInventoryAsync(userId, pulledGachaItem.ItemId);
                await SaveHistoryAsync(userId, banner.Id, pulledGachaItem, wasPity,
                    pullNumber: 1, pitySnapshot: user.PityCounter, "SinglePull", banner.CostPerSinglePull);

                user.CurrencyAmount -= banner.CostPerSinglePull;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new GachaPullResponseDto
                {
                    Results = [result],
                    GemsSpent = banner.CostPerSinglePull,
                    RemainingGems = user.CurrencyAmount,
                    CurrentPityCounter = user.PityCounter,
                    HadGuaranteedPity = wasPity
                }, $"Wish successful! You got {pulledGachaItem.Item?.Name}");
            }
            catch (Exception ex)
            {
                return Fail<GachaPullResponseDto>("Error processing wish", ex.Message);
            }
        }

        // ════════════════════════════════════════════════════
        // MULTI PULL  —  Wish x10  (1000 Gems)
        // Guarantee: ít nhất 1 item 4★+ trong 10 pulls
        // ════════════════════════════════════════════════════
        public async Task<ServiceResult<GachaPullResponseDto>> MultiPullAsync(
            Guid userId, GachaMultiPullRequest request)
        {
            try
            {
                var (user, banner, gachaItems, error) = await ValidatePullAsync(userId, request.BannerId);
                if (error != null) return Fail<GachaPullResponseDto>(error);

                if (user!.CurrencyAmount < banner!.CostPerMultiPull)
                    return Fail<GachaPullResponseDto>(
                        $"Not enough Gems. Required: {banner.CostPerMultiPull}, Available: {user.CurrencyAmount}");

                var ownedItemIds = GetOwnedItemIds(userId);
                var results = new List<GachaPullResultDto>();
                var histories = new List<(GachaItem item, bool pity, int pullNum, int pitySnap)>();

                bool hadAnyPity = false;
                bool guaranteedApplied = false;

                for (int i = 1; i <= banner.MultiPullCount; i++)
                {
                    user.PityCounter++;

                    // Pull cuối (pull thứ 10): nếu chưa có 4★+ thì force guarantee
                    bool forceGuarantee = i == banner.MultiPullCount && !HasFourStarOrAbove(results);

                    var (pulledItem, wasPity) = forceGuarantee
                        ? ForceMinimumRarity(gachaItems!, minStar: 4)
                        : RollSingleItem(gachaItems!, user.PityCounter, banner.HardPityThreshold);

                    bool triggeredPity = wasPity || forceGuarantee;
                    if (triggeredPity)
                    {
                        user.PityCounter = 0;
                        hadAnyPity = true;
                        if (forceGuarantee) guaranteedApplied = true;
                    }

                    var result = BuildResult(pulledItem, triggeredPity, ownedItemIds, pullNumber: i);
                    results.Add(result);
                    ownedItemIds.Add(pulledItem.ItemId); // mark owned trong batch này

                    histories.Add((pulledItem, triggeredPity, i, user.PityCounter));
                }

                // Lưu inventory + lịch sử
                foreach (var (item, pity, pullNum, pitySnap) in histories)
                {
                    await AddToInventoryAsync(userId, item.ItemId);
                    await SaveHistoryAsync(userId, banner.Id, item, pity,
                        pullNum, pitySnap, "MultiPull", banner.CostPerMultiPull / banner.MultiPullCount);
                }

                user.CurrencyAmount -= banner.CostPerMultiPull;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new GachaPullResponseDto
                {
                    Results = results,
                    GemsSpent = banner.CostPerMultiPull,
                    RemainingGems = user.CurrencyAmount,
                    CurrentPityCounter = user.PityCounter,
                    HadGuaranteedPity = hadAnyPity
                }, $"Wish x10 complete! Got {results.Count(r => r.StarRating >= 4)} item(s) at 4★ or above.");
            }
            catch (Exception ex)
            {
                return Fail<GachaPullResponseDto>("Error processing wish x10", ex.Message);
            }
        }

        // ════════════════════════════════════════════════════
        // USER STATUS
        // ════════════════════════════════════════════════════
        public async Task<ServiceResult<UserGachaStatusDto>> GetUserGachaStatusAsync(Guid userId, Guid bannerId)
        {
            try
            {
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return Fail<UserGachaStatusDto>("User not found");

                var banner = await _unitOfWork.GachaBanners.FirstOrDefaultAsync(b => b.Id == bannerId);
                if (banner == null) return Fail<UserGachaStatusDto>("Banner not found");

                // Pulls until 4★ guarantee = PityThreshold - (PityCounter % PityThreshold)
                int pullsUntil4Star = banner.PityThreshold - (user.PityCounter % banner.PityThreshold);
                int pullsUntil5Star = banner.HardPityThreshold - user.PityCounter;

                return Ok(new UserGachaStatusDto
                {
                    CurrentGems = user.CurrencyAmount,
                    PityCounter = user.PityCounter,
                    PullsUntilGuaranteed4Star = pullsUntil4Star,
                    PullsUntilGuaranteed5Star = Math.Max(0, pullsUntil5Star)
                }, "Status retrieved");
            }
            catch (Exception ex)
            {
                return Fail<UserGachaStatusDto>("Error", ex.Message);
            }
        }

        // ════════════════════════════════════════════════════
        // HISTORY
        // ════════════════════════════════════════════════════
        public async Task<ServiceResult<List<GachaHistoryDto>>> GetUserHistoryAsync(
            Guid userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var histories = _unitOfWork.GachaHistory
                    .GetQueryable(asNoTracking: true)
                    .Include(h => h.Item)         
                    .Include(h => h.GachaBanner)
                    .Where(h => h.UserId == userId)
                    .OrderByDescending(h => h.PulledAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var dtos = histories.Select(h => new GachaHistoryDto
                {
                    Id = h.Id,
                    BannerName = h.GachaBanner?.Name ?? string.Empty,
                    ItemName = h.Item?.Name ?? string.Empty,
                    ItemImagePath = h.Item?.ImagePath ?? string.Empty,
                    ItemCategory = h.Item?.Type ?? string.Empty,
                    StarRating = h.StarRating,
                    IsFeatured = h.IsFeatured,
                    WasPityTriggered = h.WasPityTriggered,
                    PullType = h.PullType,
                    GemsCost = h.GemsCost,
                    PulledAt = h.PulledAt
                }).ToList();

                return Ok(dtos, "History retrieved");
            }
            catch (Exception ex)
            {
                return Fail<List<GachaHistoryDto>>("Error", ex.Message);
            }
        }

        // ════════════════════════════════════════════════════
        // BANNER INFO
        // ════════════════════════════════════════════════════
        public async Task<ServiceResult<List<GachaBannerDto>>> GetActiveBannersAsync()
        {
            try
            {
                var banners = _unitOfWork.GachaBanners
                    .GetQueryable(asNoTracking: true)
                    .Where(b => b.IsActive && b.StartDate <= DateTime.Now && b.EndDate >= DateTime.Now)
                    .ToList();

                return Ok(banners.Select(MapBannerToDto).ToList(), "Active banners retrieved");
            }
            catch (Exception ex)
            {
                return Fail<List<GachaBannerDto>>("Error", ex.Message);
            }
        }

        public async Task<ServiceResult<GachaBannerDto>> GetBannerByIdAsync(Guid bannerId)
        {
            try
            {
                var banner = await _unitOfWork.GachaBanners.FirstOrDefaultAsync(b => b.Id == bannerId);
                if (banner == null) return Fail<GachaBannerDto>("Banner not found");

                return Ok(MapBannerToDto(banner), "Banner retrieved");
            }
            catch (Exception ex)
            {
                return Fail<GachaBannerDto>("Error", ex.Message);
            }
        }

        // ════════════════════════════════════════════════════
        // ADMIN — BANNER MANAGEMENT
        // ════════════════════════════════════════════════════
        public async Task<ServiceResult<GachaBannerDto>> CreateBannerAsync(CreateGachaBannerRequest request)
        {
            try
            {
                if (request.StartDate >= request.EndDate)
                    return Fail<GachaBannerDto>("EndDate must be after StartDate");

                // Validate tổng DropRate = 100
                var totalRate = request.Items.Sum(i => i.DropRate);
                if (Math.Abs(totalRate - 100.0) > 0.01)
                    return Fail<GachaBannerDto>($"Total drop rate must equal 100%. Current: {totalRate:F2}%");

                var banner = new GachaBanner
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    BannerImagePath = request.BannerImagePath,
                    CostPerSinglePull = request.CostPerSinglePull,
                    CostPerMultiPull = request.CostPerMultiPull,
                    PityThreshold = request.PityThreshold,
                    HardPityThreshold = request.HardPityThreshold,
                    IsActive = true,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    CreatedDate = DateTime.Now
                };

                await _unitOfWork.GachaBanners.AddAsync(banner);

                foreach (var itemReq in request.Items)
                {
                    await _unitOfWork.GachaItems.AddAsync(new GachaItem
                    {
                        Id = Guid.NewGuid(),
                        GachaBannerId = banner.Id,
                        ItemId = itemReq.ItemId,
                        DropRate = itemReq.DropRate,
                        StarRating = itemReq.StarRating,
                        ItemCategory = itemReq.ItemCategory,
                        IsFeatured = itemReq.IsFeatured
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                return Ok(MapBannerToDto(banner), "Banner created successfully");
            }
            catch (Exception ex)
            {
                return Fail<GachaBannerDto>("Error creating banner", ex.Message);
            }
        }

        public async Task<ServiceResult<GachaBannerDto>> UpdateBannerAsync(
            Guid bannerId, UpdateGachaBannerRequest request)
        {
            try
            {
                var banner = await _unitOfWork.GachaBanners.FirstOrDefaultAsync(b => b.Id == bannerId);
                if (banner == null) return Fail<GachaBannerDto>("Banner not found");

                banner.Name = request.Name;
                banner.Description = request.Description;
                banner.BannerImagePath = request.BannerImagePath;
                banner.CostPerSinglePull = request.CostPerSinglePull;
                banner.CostPerMultiPull = request.CostPerMultiPull;
                banner.StartDate = request.StartDate;
                banner.EndDate = request.EndDate;

                await _unitOfWork.GachaBanners.UpdateAsync(banner);
                await _unitOfWork.SaveChangesAsync();
                return Ok(MapBannerToDto(banner), "Banner updated");
            }
            catch (Exception ex)
            {
                return Fail<GachaBannerDto>("Error", ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> ToggleBannerAsync(Guid bannerId)
        {
            try
            {
                var banner = await _unitOfWork.GachaBanners.FirstOrDefaultAsync(b => b.Id == bannerId);
                if (banner == null) return Fail<bool>("Banner not found");

                banner.IsActive = !banner.IsActive;
                await _unitOfWork.GachaBanners.UpdateAsync(banner);
                await _unitOfWork.SaveChangesAsync();

                return Ok(banner.IsActive, banner.IsActive ? "Banner activated" : "Banner deactivated");
            }
            catch (Exception ex)
            {
                return Fail<bool>("Error", ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> AddItemToBannerAsync(Guid bannerId, AddGachaItemRequest request)
        {
            try
            {
                var banner = await _unitOfWork.GachaBanners.FirstOrDefaultAsync(b => b.Id == bannerId);
                if (banner == null) return Fail<bool>("Banner not found");

                var item = await _unitOfWork.Items.FirstOrDefaultAsync(i => i.Id == request.ItemId);
                if (item == null) return Fail<bool>("Item not found");

                var existingEntry = _unitOfWork.GachaItems.GetQueryable()
                    .FirstOrDefault(gi => gi.GachaBannerId == bannerId && gi.ItemId == request.ItemId);
                if (existingEntry != null) return Fail<bool>("Item already exists in this banner");

                await _unitOfWork.GachaItems.AddAsync(new GachaItem
                {
                    Id = Guid.NewGuid(),
                    GachaBannerId = bannerId,
                    ItemId = request.ItemId,
                    DropRate = request.DropRate,
                    StarRating = request.StarRating,
                    ItemCategory = request.ItemCategory,
                    IsFeatured = request.IsFeatured
                });

                await _unitOfWork.SaveChangesAsync();
                return Ok(true, "Item added to banner");
            }
            catch (Exception ex)
            {
                return Fail<bool>("Error", ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> RemoveItemFromBannerAsync(Guid bannerId, Guid itemId)
        {
            try
            {
                var gachaItem = _unitOfWork.GachaItems.GetQueryable()
                    .FirstOrDefault(gi => gi.GachaBannerId == bannerId && gi.ItemId == itemId);
                if (gachaItem == null) return Fail<bool>("Item not found in this banner");

                await _unitOfWork.GachaItems.DeleteAsync(gachaItem);
                await _unitOfWork.SaveChangesAsync();
                return Ok(true, "Item removed from banner");
            }
            catch (Exception ex)
            {
                return Fail<bool>("Error", ex.Message);
            }
        }

        // ════════════════════════════════════════════════════
        // ROLL ALGORITHM
        // ════════════════════════════════════════════════════

        /// <summary>Roll 1 item theo tỷ lệ, có soft pity và hard pity</summary>
        private (GachaItem item, bool wasPity) RollSingleItem(
            List<GachaItem> items, int pityCounter, int hardPityThreshold)
        {
            // Hard pity: bắt buộc 5★ khi đạt ngưỡng
            if (pityCounter >= hardPityThreshold)
            {
                var fiveStars = items.Where(i => i.StarRating == 5).ToList();
                if (fiveStars.Any())
                    return (fiveStars[_rng.Next(fiveStars.Count)], true);
            }

            // Soft pity: tăng tỷ lệ 5★ tuyến tính từ pull 75+
            var adjustedItems = ApplySoftPity(items, pityCounter, softPityStart: 75);

            // Weighted random
            double totalWeight = adjustedItems.Sum(x => x.weight);
            double roll = _rng.NextDouble() * totalWeight;
            double cumulative = 0;

            foreach (var (item, weight) in adjustedItems)
            {
                cumulative += weight;
                if (roll <= cumulative)
                    return (item, false);
            }

            return (items.Last(), false);
        }

        /// <summary>Force ra ít nhất item có StarRating >= minStar (cho pull thứ 10 guarantee)</summary>
        private (GachaItem item, bool wasPity) ForceMinimumRarity(List<GachaItem> items, int minStar)
        {
            var qualified = items.Where(i => i.StarRating >= minStar).ToList();
            if (!qualified.Any())
                qualified = items.OrderByDescending(i => i.StarRating).ToList();

            // Weighted random trong nhóm qualified
            double total = qualified.Sum(i => i.DropRate);
            double roll = _rng.NextDouble() * total;
            double cum = 0;
            foreach (var item in qualified)
            {
                cum += item.DropRate;
                if (roll <= cum) return (item, true);
            }
            return (qualified.Last(), true);
        }

        private List<(GachaItem item, double weight)> ApplySoftPity(
            List<GachaItem> items, int pityCounter, int softPityStart)
        {
            if (pityCounter < softPityStart)
                return items.Select(i => (i, i.DropRate)).ToList();

            // Mỗi pull sau softPityStart tăng 6% tỷ lệ 5★
            double boost = (pityCounter - softPityStart + 1) * 6.0;
            double extraFor5Star = Math.Min(boost, 50.0); // cap 50% boost

            return items.Select(i =>
            {
                if (i.StarRating == 5)
                    return (i, i.DropRate + extraFor5Star);
                if (i.StarRating == 3)
                    return (i, Math.Max(0.1, i.DropRate - extraFor5Star));
                return (i, i.DropRate);
            }).ToList();
        }

        // ════════════════════════════════════════════════════
        // HELPERS
        // ════════════════════════════════════════════════════
        private async Task<(User? user, GachaBanner? banner, List<GachaItem>? items, string? error)>
            ValidatePullAsync(Guid userId, Guid bannerId)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return (null, null, null, "User not found");
            if (user.IsBanned) return (null, null, null, "Your account has been banned");

            var banner = await _unitOfWork.GachaBanners.FirstOrDefaultAsync(b => b.Id == bannerId);
            if (banner == null) return (null, null, null, "Banner not found");
            if (!banner.IsActive) return (null, null, null, "This banner is not active");
            if (DateTime.Now < banner.StartDate || DateTime.Now > banner.EndDate)
                return (null, null, null, "This banner is not available right now");

            var gachaItems = _unitOfWork.GachaItems
                .GetQueryable(asNoTracking: true)
                .Include(gi => gi.Item)
                .Where(gi => gi.GachaBannerId == bannerId)
                .ToList();

            if (!gachaItems.Any())
                return (null, null, null, "No items configured in this banner");

            return (user, banner, gachaItems, null);
        }

        private HashSet<Guid> GetOwnedItemIds(Guid userId) =>
            _unitOfWork.UserItems
                .GetQueryable(asNoTracking: true)
                .Where(ui => ui.UserId == userId)
                .Select(ui => ui.ItemId)
                .ToHashSet();

        private async Task AddToInventoryAsync(Guid userId, Guid itemId)
        {
            var existing = await _unitOfWork.UserItems
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ItemId == itemId);

            if (existing != null)
            {
                existing.Quantity += 1;
                await _unitOfWork.UserItems.UpdateAsync(existing);
            }
            else
            {
                await _unitOfWork.UserItems.AddAsync(new UserItem
                {
                    UserId = userId,
                    ItemId = itemId,
                    Quantity = 1
                });
            }
        }

        private async Task SaveHistoryAsync(Guid userId, Guid bannerId, GachaItem gachaItem,
            bool wasPity, int pullNumber, int pitySnapshot, string pullType, int cost)
        {
            await _unitOfWork.GachaHistory.AddAsync(new GachaHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GachaBannerId = bannerId,
                ItemId = gachaItem.ItemId,
                StarRating = gachaItem.StarRating,
                IsFeatured = gachaItem.IsFeatured,
                WasPityTriggered = wasPity,
                PullNumberInSession = pullNumber,
                PityCounterSnapshot = pitySnapshot,
                PullType = pullType,
                GemsCost = cost,
                PulledAt = DateTime.Now
            });
        }

        private static bool HasFourStarOrAbove(List<GachaPullResultDto> results) =>
            results.Any(r => r.StarRating >= 4);

        private static GachaPullResultDto BuildResult(
            GachaItem gi, bool wasPity, HashSet<Guid> ownedIds, int pullNumber) => new()
            {
                ItemId = gi.ItemId,
                ItemName = gi.Item?.Name ?? string.Empty,
                ItemCategory = gi.ItemCategory,
                ImagePath = gi.Item?.ImagePath ?? string.Empty,
                StarRating = gi.StarRating,
                IsFeatured = gi.IsFeatured,
                WasPityTriggered = wasPity,
                IsNew = !ownedIds.Contains(gi.ItemId),
                PullNumber = pullNumber
            };

        private static GachaBannerDto MapBannerToDto(GachaBanner b) => new()
        {
            Id = b.Id,
            Name = b.Name,
            Description = b.Description,
            BannerImagePath = b.BannerImagePath,
            CostPerSinglePull = b.CostPerSinglePull,
            CostPerMultiPull = b.CostPerMultiPull,
            PityThreshold = b.PityThreshold,
            HardPityThreshold = b.HardPityThreshold,
            IsActive = b.IsActive,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            FeaturedItems = b.GachaItems
                .Where(gi => gi.IsFeatured)
                .Select(gi => new GachaBannerItemDto
                {
                    ItemId = gi.ItemId,
                    ItemName = gi.Item?.Name ?? string.Empty,
                    ItemCategory = gi.ItemCategory,
                    ImagePath = gi.Item?.ImagePath ?? string.Empty,
                    StarRating = gi.StarRating,
                    DropRate = gi.DropRate,
                    IsFeatured = true
                }).ToList(),
            AllItems = b.GachaItems.Select(gi => new GachaBannerItemDto
            {
                ItemId = gi.ItemId,
                ItemName = gi.Item?.Name ?? string.Empty,
                ItemCategory = gi.ItemCategory,
                ImagePath = gi.Item?.ImagePath ?? string.Empty,
                StarRating = gi.StarRating,
                DropRate = gi.DropRate,
                IsFeatured = gi.IsFeatured
            }).ToList()
        };

        private static ServiceResult<T> Ok<T>(T data, string msg) =>
            new() { Success = true, Message = msg, Data = data };

        private static ServiceResult<T> Fail<T>(string msg, string? detail = null) =>
            new() { Success = false, Message = msg, Errors = detail != null ? [detail] : [] };
    }
}