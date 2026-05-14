using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LenarSoft.Models
{
    /// Основной контейнер для всех данных из Excel
    public class ProjectData
    {
        public List<ControlCable> ControlCables { get; set; } = new();
        public List<NetworkConnection> NetworkConnections { get; set; } = new();
        public List<PowerCable> PowerCables { get; set; } = new();


        /// Данные, сгруппированные по шкафам для удобной генерации
        public Dictionary<string, CabinetData> Cabinets { get; set; } = new();


        /// Общая статистика по проекту
        public ProjectStatistics GetStatistics() => new()
        {
            TotalSignals = ControlCables.Count,
            TotalNetworkConnections = NetworkConnections.Count,
            TotalPowerCables = PowerCables.Count,
            TotalCabinets = Cabinets.Count
        };
    }

    /// Статистика проекта
    public class ProjectStatistics
    {
        public int TotalSignals { get; set; }
        public int TotalNetworkConnections { get; set; }
        public int TotalPowerCables { get; set; }
        public int TotalCabinets { get; set; }
    }
}
