using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenarSoft.Models
{
    /// Кабель питания - лист "Питание"
    public class PowerCable : IHasPageName, ICable
    {
        public int Number { get; set; }
        public string Description { get; set; } = string.Empty;

        // Откуда
        public string SourceLocation { get; set; } = string.Empty;
        public string SourceEquipment { get; set; } = string.Empty;
        public string SourceTerminalBlock { get; set; } = string.Empty;
        public string SourceTerminals { get; set; } = string.Empty;

        public string Voltage { get; set; } = string.Empty;

        // Кабель (реализация ICable)
        public string CableNumber { get; set; } = string.Empty;
        public string CableType { get; set; } = string.Empty;
        public double CableLength { get; set; }
        public string CoreNumbers
        {
            get => string.Empty;
            set { } // можно ничего не делать или игнорировать
        }

        // Куда
        public string DestLocation { get; set; } = string.Empty;
        public string DestEquipment { get; set; } = string.Empty;
        public string DestTerminalBlock { get; set; } = string.Empty;
        public string DestTerminals { get; set; } = string.Empty;

        public string GetPageName() => $"={SourceLocation}+PWR/{Number}";

        public Dictionary<string, string> GetMacroProperties()
        {
            return new Dictionary<string, string>
            {
                ["Description"] = Description,
                ["SourceEquipment"] = SourceEquipment,
                ["SourceTerminals"] = $"{SourceTerminalBlock}:{SourceTerminals}",
                ["DestEquipment"] = DestEquipment,
                ["DestTerminals"] = $"{DestTerminalBlock}:{DestTerminals}",
                ["Voltage"] = Voltage,
                ["CableNumber"] = CableNumber,
                ["CableType"] = CableType
            };
        }
    }
}
