using BussinessObjects.Models;
using DataAccess.IRepositories;
using Services.IServices;

namespace Services.Services
{
    public class NPCService : INPCService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NPCService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<List<NPCDto>> GetAll()
        {
            try
            {
                var npcList = _unitOfWork.NPCs.GetQueryable(asNoTracking: true).ToList();

                if (!npcList.Any())
                    return new ServiceResult<List<NPCDto>>
                    {
                        Success = true,
                        Message = "No NPCs found",
                        Data = new List<NPCDto>()
                    };

                var dtoList = npcList.Select(n => new NPCDto
                {
                    Id = n.Id,
                    Name = n.Name,
                    Description = n.Description,
                    ImagePath = n.ImagePath,
                    Location = n.Location,
                    NPCType = n.NPCType,
                    CreatedDate = n.CreatedDate,
                    UpdatedDate = n.UpdatedDate
                }).ToList();

                return new ServiceResult<List<NPCDto>>
                {
                    Success = true,
                    Message = "NPCs retrieved successfully",
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<NPCDto>>
                {
                    Success = false,
                    Message = "Error retrieving NPCs",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<NPCDto>> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<NPCDto>
                    {
                        Success = false,
                        Message = "Invalid NPC ID"
                    };

                var npc = await _unitOfWork.NPCs.FirstOrDefaultAsync(n => n.Id == id);

                if (npc == null)
                    return new ServiceResult<NPCDto>
                    {
                        Success = false,
                        Message = "NPC not found"
                    };

                var dto = new NPCDto
                {
                    Id = npc.Id,
                    Name = npc.Name,
                    Description = npc.Description,
                    ImagePath = npc.ImagePath,
                    Location = npc.Location,
                    NPCType = npc.NPCType,
                    CreatedDate = npc.CreatedDate,
                    UpdatedDate = npc.UpdatedDate
                };

                return new ServiceResult<NPCDto>
                {
                    Success = true,
                    Message = "NPC retrieved successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<NPCDto>
                {
                    Success = false,
                    Message = "Error retrieving NPC",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<NPCDto>> CreateAsync(CreateNPCRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name) ||
                    string.IsNullOrWhiteSpace(request.ImagePath) ||
                    string.IsNullOrWhiteSpace(request.NPCType))
                {
                    return new ServiceResult<NPCDto>
                    {
                        Success = false,
                        Message = "Name, ImagePath, and NPCType are required",
                        Errors = ["Name, ImagePath, and NPCType cannot be empty"]
                    };
                }

                var existingNPC = await _unitOfWork.NPCs.FirstOrDefaultAsync(n => n.Name == request.Name);
                if (existingNPC != null)
                    return new ServiceResult<NPCDto>
                    {
                        Success = false,
                        Message = "An NPC with this name already exists"
                    };

                var npc = new NPC
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    ImagePath = request.ImagePath,
                    Location = request.Location,
                    NPCType = request.NPCType,
                    CreatedDate = DateTime.Now
                };

                await _unitOfWork.NPCs.AddAsync(npc);
                await _unitOfWork.SaveChangesAsync();

                var dto = new NPCDto
                {
                    Id = npc.Id,
                    Name = npc.Name,
                    Description = npc.Description,
                    ImagePath = npc.ImagePath,
                    Location = npc.Location,
                    NPCType = npc.NPCType,
                    CreatedDate = npc.CreatedDate,
                    UpdatedDate = npc.UpdatedDate
                };

                return new ServiceResult<NPCDto>
                {
                    Success = true,
                    Message = "NPC created successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<NPCDto>
                {
                    Success = false,
                    Message = "Error creating NPC",
                    Errors = [ex.Message]
                };
            }
        }

        public async Task<ServiceResult<NPCDto>> UpdateAsync(Guid id, UpdateNPCRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ServiceResult<NPCDto>
                    {
                        Success = false,
                        Message = "Invalid NPC ID"
                    };

                var npc = await _unitOfWork.NPCs.FirstOrDefaultAsync(n => n.Id == id);
                if (npc == null)
                    return new ServiceResult<NPCDto>
                    {
                        Success = false,
                        Message = "NPC not found"
                    };

                if (string.IsNullOrWhiteSpace(request.Name) ||
                    string.IsNullOrWhiteSpace(request.ImagePath) ||
                    string.IsNullOrWhiteSpace(request.NPCType))
                {
                    return new ServiceResult<NPCDto>
                    {
                        Success = false,
                        Message = "Name, ImagePath, and NPCType are required",
                        Errors = ["Name, ImagePath, and NPCType cannot be empty"]
                    };
                }

                // Check if name is changed and if new name already exists
                if (npc.Name != request.Name)
                {
                    var existingNPC = await _unitOfWork.NPCs.FirstOrDefaultAsync(n => n.Name == request.Name);
                    if (existingNPC != null)
                        return new ServiceResult<NPCDto>
                        {
                            Success = false,
                            Message = "An NPC with this name already exists"
                        };
                }

                npc.Name = request.Name;
                npc.Description = request.Description;
                npc.ImagePath = request.ImagePath;
                npc.Location = request.Location;
                npc.NPCType = request.NPCType;
                npc.UpdatedDate = DateTime.Now;

                await _unitOfWork.NPCs.UpdateAsync(npc);
                await _unitOfWork.SaveChangesAsync();

                var dto = new NPCDto
                {
                    Id = npc.Id,
                    Name = npc.Name,
                    Description = npc.Description,
                    ImagePath = npc.ImagePath,
                    Location = npc.Location,
                    NPCType = npc.NPCType,
                    CreatedDate = npc.CreatedDate,
                    UpdatedDate = npc.UpdatedDate
                };

                return new ServiceResult<NPCDto>
                {
                    Success = true,
                    Message = "NPC updated successfully",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<NPCDto>
                {
                    Success = false,
                    Message = "Error updating NPC",
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
                        Message = "Invalid NPC ID"
                    };

                var npc = await _unitOfWork.NPCs.FirstOrDefaultAsync(n => n.Id == id);
                if (npc == null)
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "NPC not found"
                    };

                await _unitOfWork.NPCs.DeleteAsync(npc);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<bool>
                {
                    Success = true,
                    Message = "NPC deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Error deleting NPC",
                    Errors = [ex.Message]
                };
            }
        }
    }
}
