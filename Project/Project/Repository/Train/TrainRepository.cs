using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.Models;

namespace Project.Repository.Train
{
    public class TrainRepository : ITrainRepository
    {
        private readonly FastRailDbContext _dbContext;
        private IMapper _mapper;

        public TrainRepository(FastRailDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<(bool Success, string Message, int TrainId)> CreateAsync(TrainDTO dto)
        {
            var train = _mapper.Map<Models.Train>(dto);

            _dbContext.Train.Add(train);
            await _dbContext.SaveChangesAsync();

            return (true, "Created", train.TrainId);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var train= GetByIdAsync(id);
            if (train != null)
            {
                var trainMapped= _mapper.Map<Models.Train>(train);
                _dbContext.Train.Remove(trainMapped);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<IEnumerable<TrainDTO>> GetAllAsync()
        {
            return await _dbContext.Train.ProjectTo<TrainDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<TrainDTO?> GetByIdAsync(int id)
        {
            var train = await _dbContext.Train.FindAsync(id);
            return train == null ? null : _mapper.Map<TrainDTO>(train);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, TrainDTO dto)
        {
            var train = await _dbContext.Train.FindAsync(id);
            if (train == null)
                return (false, "Train not found");

            
            train.TrainNumber = dto.TrainNumber;
            train.TrainName = dto.TrainName;
            train.TrainType = dto.TrainType;
            train.TotalCarriages = dto.TotalCarriages;
            train.MaxSpeed = dto.MaxSpeed;
            train.Manufacturer = dto.Manufacturer;
            train.YearOfManufacture = dto.YearOfManufacture;
            train.IsActive = dto.IsActive;

            await _dbContext.SaveChangesAsync();
            return (true, "Train updated successfully");
        }
    }
}
