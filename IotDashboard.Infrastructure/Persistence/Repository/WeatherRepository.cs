using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Infrastructure.Persistence.Repository
{
    public class WeatherRepository : BaseRepository<Weather>, IWeatherRepository
    {
        public WeatherRepository(AppDBContext context):base(context)
        {
            
        }
    }
}
