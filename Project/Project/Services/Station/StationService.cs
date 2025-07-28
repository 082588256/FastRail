
namespace Project.Services.Station
{
    public class StationService : IStationService
    {
        private readonly FastRailDbContext _context;

        public StationService(FastRailDbContext context)
        {
            _context = context;
        }
        public List<Models.Station> GetStations()
        {
            var result= _context.Station.ToList();
            return result;
        }
    }
}
