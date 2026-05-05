using BussinessObjects.DTOs.Gacha;
using BussinessObjects.Models;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Services.IServices;

namespace Services.Services
{
    public class GachaService : IGachaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Random _rng = new();
        private readonly IWebHostEnvironment _env;
        private readonly IFirebaseStorageService _firebaseStorage;

        public GachaService(IUnitOfWork unitOfWork, IWebHostEnvironment env, IFirebaseStorageService firebaseStorage)
        {
            _unitOfWork = unitOfWork;
            _env = env;
            _firebaseStorage = firebaseStorage;
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

                var ownedItemIds = GetOwnedItemIds(userId);

                user.PityCounter++;
                var (pulledGachaItem, wasPity) = RollSingleItem(gachaItems!, user.PityCounter, banner.HardPityThreshold);

                // Reset pity khi và chỉ khi ra 5★ (hard pity, soft pity, hay lucky đều reset)
                if (pulledGachaItem.StarRating == 5)
                    user.PityCounter = 0;

                var result = BuildResult(pulledGachaItem, wasPity, ownedItemIds, pullNumber: 1);

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

                for (int i = 1; i <= banner.MultiPullCount; i++)
                {
                    user.PityCounter++;

                    // Pull cuối (pull thứ 10): nếu chưa có 4★+ thì force guarantee
                    bool forceGuarantee = i == banner.MultiPullCount && !HasFourStarOrAbove(results);

                    GachaItem pulledItem;
                    bool wasPity;

                    if (forceGuarantee)
                    {
                        (pulledItem, wasPity) = ForceMinimumRarity(gachaItems!, minStar: 4);
                    }
                    else
                    {
                        (pulledItem, wasPity) = RollSingleItem(gachaItems!, user.PityCounter, banner.HardPityThreshold);
                    }

                    // Reset pity khi và chỉ khi ra 5★
                    if (pulledItem.StarRating == 5)
                    {
                        user.PityCounter = 0;
                        hadAnyPity = true;
                        wasPity = true; // đảm bảo flag nhất quán khi ra 5★ bất kỳ cách nào
                    }

                    var result = BuildResult(pulledItem, wasPity, ownedItemIds, pullNumber: i);
                    results.Add(result);
                    ownedItemIds.Add(pulledItem.ItemId); // mark owned trong batch này

                    histories.Add((pulledItem, wasPity, i, user.PityCounter));
                }

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

                int pullsUntil4Star = banner.PityThreshold - (user.PityCounter % banner.PityThreshold);
                int pullsUntil5Star = Math.Max(0, banner.HardPityThreshold - user.PityCounter);

                return Ok(new UserGachaStatusDto
                {
                    CurrentGems = user.CurrencyAmount,
                    PityCounter = user.PityCounter,
                    PullsUntilGuaranteed4Star = pullsUntil4Star,
                    PullsUntilGuaranteed5Star = pullsUntil5Star
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
        // BANNER INFO  (public) — chỉ banner đang active + trong hạn
        // ════════════════════════════════════════════════════
        public async Task<ServiceResult<List<GachaBannerDto>>> GetActiveBannersAsync()
        {
            try
            {
                var now = DateTime.Now;
                var banners = _unitOfWork.GachaBanners
                    .GetQueryable(asNoTracking: true)
                    .Include(b => b.GachaItems)
                        .ThenInclude(gi => gi.Item)
                    .Where(b => b.IsActive && b.StartDate <= now && b.EndDate >= now)
                    .OrderByDescending(b => b.CreatedDate)
                    .ToList();

                return Ok(banners.Select(MapBannerToDto).ToList(),
                    $"{banners.Count} active banner(s) retrieved");
            }
            catch (Exception ex)
            {
                return Fail<List<GachaBannerDto>>("Error", ex.Message);
            }
        }

        // ════════════════════════════════════════════════════
        // BANNER INFO  (admin) — TẤT CẢ banner, không filter
        // GET /api/admin/gacha/banners
        // ════════════════════════════════════════════════════
        public async Task<ServiceResult<List<GachaBannerDto>>> GetAllBannersForAdminAsync()
        {
            try
            {
                var banners = _unitOfWork.GachaBanners
                    .GetQueryable(asNoTracking: true)
                    .Include(b => b.GachaItems)
                        .ThenInclude(gi => gi.Item)
                    // Không filter IsActive / StartDate / EndDate
                    // Admin phải thấy toàn bộ: active, inactive, hết hạn, chưa bắt đầu
                    .OrderByDescending(b => b.CreatedDate)
                    .ToList();

                return Ok(banners.Select(MapBannerToDto).ToList(),
                    $"{banners.Count} banner(s) retrieved");
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
                var banner = await _unitOfWork.GachaBanners
                    .GetQueryable(asNoTracking: true)
                    .Include(b => b.GachaItems)
                        .ThenInclude(gi => gi.Item)
                    .FirstOrDefaultAsync(b => b.Id == bannerId);

                if (banner == null) return Fail<GachaBannerDto>("Banner not found");

                return Ok(MapBannerToDto(banner), "Banner retrieved");
            }
            catch (Exception ex)
            {
                return Fail<GachaBannerDto>("Error", ex.Message);
            }
        }

        // ════════════════════════════════════════════════════
        // ADMIN — ITEMS AVAILABLE
        // GET /api/admin/gacha/items-available
        // ════════════════════════════════════════════════════
        public async Task<ServiceResult<List<AvailableItemDto>>> GetAvailableItemsAsync(string? search = null)
        {
            try
            {
                var query = _unitOfWork.Items.GetQueryable(asNoTracking: true);

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(i => i.Name.Contains(search));

                var items = query
                    .OrderBy(i => i.Name)
                    .Select(i => new AvailableItemDto
                    {
                        ItemId = i.Id,
                        Name = i.Name,
                        ImagePath = i.ImagePath ?? string.Empty,
                        Type = i.Type ?? string.Empty
                    })
                    .ToList();

                return Ok(items, $"{items.Count} item(s) available");
            }
            catch (Exception ex)
            {
                return Fail<List<AvailableItemDto>>("Error fetching available items", ex.Message);
            }
        }

        // ════════════════════════════════════════════════════
        // ADMIN — CREATE BANNER
        // POST /api/admin/gacha/banners
        // ════════════════════════════════════════════════════
        public async Task<ServiceResult<GachaBannerDto>> CreateBannerAsync(CreateGachaBannerRequest request)
        {
            try
            {
                // 1. Validate date range
                if (request.StartDate >= request.EndDate)
                    return Fail<GachaBannerDto>("EndDate must be after StartDate");

                // 2. Validate total drop rate (chỉ khi có item)
                if (request.Items.Any())
                {
                    var totalRate = request.Items.Sum(i => i.DropRate);
                    if (Math.Abs(totalRate - 100.0) > 0.01)
                        return Fail<GachaBannerDto>(
                            $"Total drop rate must equal 100%. Current: {totalRate:F2}%");
                }

                // 3. Validate từng ItemId có tồn tại trong DB
                var invalidItemIds = await ValidateItemIdsExistAsync(
                    request.Items.Select(i => i.ItemId).ToList());

                if (invalidItemIds.Count > 0)
                    return Fail<GachaBannerDto>(
                        $"The following ItemId(s) do not exist in the database: " +
                        $"{string.Join(", ", invalidItemIds)}");

                // 4. Xử lý ảnh với logic ưu tiên
                var (bannerImagePath, imageError) = await ResolveBannerImageAsync(
                    request.ImageFile, request.BannerImagePath, allowEmpty: false);

                if (imageError != null)
                    return Fail<GachaBannerDto>(imageError);

                // 5. Tạo Banner
                var banner = new GachaBanner
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    BannerImagePath = bannerImagePath!,
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

                // 6. Tạo GachaItem từ Item có sẵn
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

        // ════════════════════════════════════════════════════
        // ADMIN — UPDATE BANNER
        // PUT /api/admin/gacha/banners/{bannerId}
        // ════════════════════════════════════════════════════
        public async Task<ServiceResult<GachaBannerDto>> UpdateBannerAsync(
            Guid bannerId, UpdateGachaBannerRequest request)
        {
            try
            {
                var banner = await _unitOfWork.GachaBanners
                    .GetQueryable()
                    .Include(b => b.GachaItems)
                        .ThenInclude(gi => gi.Item)
                    .FirstOrDefaultAsync(b => b.Id == bannerId);

                if (banner == null) return Fail<GachaBannerDto>("Banner not found");

                if (request.StartDate >= request.EndDate)
                    return Fail<GachaBannerDto>("EndDate must be after StartDate");

                // allowEmpty: true → nếu không cung cấp ảnh mới thì giữ ảnh cũ
                var (resolvedPath, imageError) = await ResolveBannerImageAsync(
                    request.ImageFile, request.BannerImagePath, allowEmpty: true);

                if (imageError != null)
                    return Fail<GachaBannerDto>(imageError);

                banner.Name = request.Name;
                banner.Description = request.Description;
                banner.BannerImagePath = resolvedPath ?? banner.BannerImagePath;
                banner.CostPerSinglePull = request.CostPerSinglePull;
                banner.CostPerMultiPull = request.CostPerMultiPull;
                banner.StartDate = request.StartDate;
                banner.EndDate = request.EndDate;

                await _unitOfWork.GachaBanners.UpdateAsync(banner);
                await _unitOfWork.SaveChangesAsync();
                return Ok(MapBannerToDto(banner), "Banner updated successfully");
            }
            catch (Exception ex)
            {
                return Fail<GachaBannerDto>("Error updating banner", ex.Message);
            }
        }

        // ════════════════════════════════════════════════════
        // ADMIN — TOGGLE BANNER
        // PATCH /api/admin/gacha/banners/{bannerId}/toggle
        // ════════════════════════════════════════════════════
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

        // ════════════════════════════════════════════════════
        // ADMIN — ADD ITEM TO BANNER
        // POST /api/admin/gacha/banners/{bannerId}/items
        // ════════════════════════════════════════════════════
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

        // ════════════════════════════════════════════════════
        // ADMIN — REMOVE ITEM FROM BANNER
        // DELETE /api/admin/gacha/banners/{bannerId}/items/{itemId}
        // ════════════════════════════════════════════════════
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
        // ROLL ALGORITHM  —  GIỮ NGUYÊN
        // ════════════════════════════════════════════════════

        /// <summary>
        /// Roll 1 item theo tỷ lệ, có soft pity và hard pity.
        /// wasPity = true chỉ khi hard pity kích hoạt.
        /// Việc reset PityCounter dựa vào StarRating == 5 ở caller, không phải wasPity.
        /// </summary>
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

            double boost = (pityCounter - softPityStart + 1) * 6.0;
            double extraFor5Star = Math.Min(boost, 50.0);

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
        // PRIVATE HELPERS — Pull
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

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Banner Image (Firebase Storage)
        // ════════════════════════════════════════════════════

        /// <summary>
        /// Resolve ảnh banner theo thứ tự ưu tiên:
        ///   1. ImageFile  → upload lên Firebase Storage, trả về public URL
        ///   2. BannerImagePath → validate URL rồi dùng trực tiếp
        ///   3. Cả hai null/empty:
        ///      - allowEmpty = false → trả lỗi (bắt buộc khi Create)
        ///      - allowEmpty = true  → trả (null, null), caller giữ ảnh cũ (dùng khi Update)
        /// </summary>
        private async Task<(string? path, string? error)> ResolveBannerImageAsync(
            IFormFile? imageFile,
            string? bannerImagePath,
            bool allowEmpty = false)
        {
            // Ưu tiên 1: File upload → đẩy lên Firebase Storage
            if (imageFile is { Length: > 0 })
            {
                try
                {
                    var publicUrl = await SaveBannerImageFileAsync(imageFile);
                    return (publicUrl, null);
                }
                catch (Exception ex)
                {
                    return (null, $"Failed to upload image to Firebase Storage: {ex.Message}");
                }
            }

            // Ưu tiên 2: Path/URL do admin nhập thủ công
            if (!string.IsNullOrWhiteSpace(bannerImagePath))
            {
                var validationError = ValidateBannerImagePath(bannerImagePath);
                if (validationError != null)
                    return (null, validationError);

                return (bannerImagePath.Trim(), null);
            }

            // Ưu tiên 3: Không có gì
            if (allowEmpty)
                return (null, null); // caller sẽ giữ ảnh cũ

            return (null,
                "Banner image is required. " +
                "Provide either an image file (multipart upload) or a valid image URL (https://...).");
        }

        /// <summary>
        /// Upload file ảnh lên Firebase Storage và trả về public URL.
        /// KHÔNG lưu vào local disk — tránh lỗi 404 trên Azure App Service (ephemeral filesystem).
        /// </summary>
        private async Task<string> SaveBannerImageFileAsync(IFormFile imageFile)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";

            await using var stream = imageFile.OpenReadStream();

            // Upload lên Firebase Storage vào folder "banners"
            // Trả về URL dạng: https://firebasestorage.googleapis.com/v0/b/.../o/banners%2F{fileName}?alt=media
            var publicUrl = await _firebaseStorage.UploadFileAsync(
                stream,
                uniqueFileName,
                folderPath: "banners");

            return publicUrl;
        }

        /// <summary>
        /// Validate URL ảnh do admin nhập tay.
        /// Chỉ chấp nhận absolute URL (http/https) — không còn hỗ trợ server-relative path.
        /// </summary>
        private static string? ValidateBannerImagePath(string path)
        {
            var trimmed = path.Trim();

            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                if (!Uri.TryCreate(trimmed, UriKind.Absolute, out _))
                    return $"Invalid image URL: '{trimmed}'";

                return null;
            }

            return $"Invalid image path: '{trimmed}'. " +
                   "Must be an absolute URL (https://...). " +
                   "Tip: Upload the image file directly instead of providing a path.";
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Admin Item Validation
        // ════════════════════════════════════════════════════

        /// <summary>
        /// Bulk-check danh sách ItemId có tồn tại trong DB không (1 query duy nhất).
        /// Trả về list các ID không hợp lệ — empty list nghĩa là tất cả hợp lệ.
        /// </summary>
        private async Task<List<Guid>> ValidateItemIdsExistAsync(List<Guid> itemIds)
        {
            if (itemIds.Count == 0) return [];

            var existingIds = _unitOfWork.Items
                .GetQueryable(asNoTracking: true)
                .Where(i => itemIds.Contains(i.Id))
                .Select(i => i.Id)
                .ToHashSet();

            return itemIds.Where(id => !existingIds.Contains(id)).ToList();
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Mapping
        // ════════════════════════════════════════════════════
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

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — ServiceResult factory
        // ════════════════════════════════════════════════════
        private static ServiceResult<T> Ok<T>(T data, string msg) =>
            new() { Success = true, Message = msg, Data = data };

        private static ServiceResult<T> Fail<T>(string msg, string? detail = null) =>
            new() { Success = false, Message = msg, Errors = detail != null ? [detail] : [] };
    }
}