using Project.Models;

namespace Project.Services.Station
{
    public interface IStationService
    {
        public List<Models.Station> GetStations();
    }
}
