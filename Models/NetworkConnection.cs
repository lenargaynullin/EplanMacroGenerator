using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenarSoft.Models
{
    /// Сетевое подключение - лист "Интерфейсы"
    public class NetworkConnection : IHasPageName, ICable
    {
        public int Number { get; set; }
        public string Description { get; set; } = string.Empty;

        // Откуда
        public string SourceLocation { get; set; } = string.Empty;
        public string SourceEquipment { get; set; } = string.Empty;
        public string SourceDevice { get; set; } = string.Empty;
        public string SourceContacts { get; set; } = string.Empty;

        public string ConnectionType { get; set; } = string.Empty; // SYSTEM, FO, TP

        // Кабель (реализация ICable)
        public string CableNumber { get; set; } = string.Empty;
        public string CableType { get; set; } = string.Empty;
        public double CableLength { get; set; }
        public string CoreNumbers { get; set; } = string.Empty;

        // Куда
        public string DestLocation { get; set; } = string.Empty;
        public string DestEquipment { get; set; } = string.Empty;
        public string DestDevice { get; set; } = string.Empty;
        public string DestContacts { get; set; } = string.Empty;

        // Дополнительно
        public string Spec { get; set; } = string.Empty;
        public string CableTag { get; set; } = string.Empty;

        public string GetPageName() => $"={SourceLocation}+NET/{Number}";

        public Dictionary<string, string> GetMacroProperties()
        {
            return new Dictionary<string, string>
            {
                ["Description"] = Description,
                ["SourceDevice"] = SourceDevice,
                ["SourceContacts"] = SourceContacts,
                ["DestDevice"] = DestDevice,
                ["DestContacts"] = DestContacts,
                ["ConnectionType"] = ConnectionType,
                ["CableNumber"] = CableNumber,
                ["CableType"] = CableType,
                ["CoreNumbers"] = CoreNumbers,
                ["CableTag"] = CableTag
            };
        }
    }
}
