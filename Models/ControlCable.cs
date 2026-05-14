using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenarSoft.Models
{
    /// Контрольный кабель (сигналы от датчиков) - лист "Физика"
    public class ControlCable : IHasPageName, ICable
    {
        // Основная информация
        public int Number { get; set; }
        public string LocationTitle { get; set; } = string.Empty;
        public string SignalName { get; set; } = string.Empty;
        public string SignalTag { get; set; } = string.Empty;
        public string SignalType { get; set; } = string.Empty;
        public string SignalChar { get; set; } = string.Empty;
        public string Safety { get; set; } = string.Empty;
        public string Hart { get; set; } = string.Empty;
        public string SchemeCode { get; set; } = string.Empty;

        // Датчик (источник)
        public string SensorTag { get; set; } = string.Empty;

        // Кабель (реализация ICable)
        public string CableNumber { get; set; } = string.Empty;
        public string CableType { get; set; } = string.Empty;
        public double CableLength { get; set; }
        public string CoreNumbers { get; set; } = string.Empty;

        // Шкаф кроссовый
        public string CabinetLocation { get; set; } = string.Empty;
        public string CabinetTag { get; set; } = string.Empty;
        public string TerminalBlock { get; set; } = string.Empty;
        public string Terminals { get; set; } = string.Empty;

        // FE (заземление)
        public string FeTerminalBlock { get; set; } = string.Empty;
        public string FeTerminals { get; set; } = string.Empty;

        // БИЗ (барьеры искрозащиты)
        public string BizTag { get; set; } = string.Empty;
        public string BizContacts { get; set; } = string.Empty;

        // Устройство в шкафу (FTA)
        public string DeviceTag { get; set; } = string.Empty;
        public string DeviceContacts { get; set; } = string.Empty;

        // Контроллер
        public string ControllerModule { get; set; } = string.Empty;
        public int ModuleChannel { get; set; }
        public string ControllerCabinet { get; set; } = string.Empty;
        public string ControllerSignalTag { get; set; } = string.Empty;

        // Единицы измерения и шкала
        public string? Units { get; set; }
        public string? Scale { get; set; }

        // Реализация интерфейсов
        public string GetPageName() => $"={CabinetLocation}+{TerminalBlock}/{SignalType}_{Number}";

        /// Получить DT клеммы в шкафу
        public string GetTerminalDT() => $"={CabinetLocation}+{TerminalBlock}:{Terminals}";

        /// Получить словарь свойств для подстановки в макрос EPLAN
        public Dictionary<string, string> GetMacroProperties()
        {
            return new Dictionary<string, string>
            {
                ["SignalTag"] = SignalTag,
                ["SignalName"] = SignalName,
                ["SignalType"] = SignalType,
                ["SignalChar"] = SignalChar,
                ["SensorTag"] = SensorTag,
                ["CableNumber"] = CableNumber,
                ["CableType"] = CableType,
                ["CoreNumbers"] = CoreNumbers,
                ["DeviceTag"] = DeviceTag,
                ["DeviceContacts"] = DeviceContacts,
                ["ControllerModule"] = ControllerModule,
                ["ModuleChannel"] = ModuleChannel.ToString(),
                ["Units"] = Units ?? string.Empty,
                ["Scale"] = Scale ?? string.Empty
            };
        }
    }
}
