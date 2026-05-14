using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenarSoft.Models
{
    /// Отдельная клемма
    public class Terminal
    {
        /// Номер клеммы
        public int Number { get; set; }

        /// Потенциал (например, +24V, 0V)
        public string Potential { get; set; } = string.Empty;

        /// Имя кабеля, подключенного к клемме
        public string CableName { get; set; } = string.Empty;

        /// Назначение (куда идет сигнал)
        public string Destination { get; set; } = string.Empty;

        /// Примечание или дополнительная информация
        public string? Remarks { get; set; }

        /// Словарь свойств для подстановки в макрос EPLAN
        public Dictionary<string, string> GetMacroProperties()
        {
            return new Dictionary<string, string>
            {
                ["TerminalNumber"] = Number.ToString(),
                ["Potential"] = Potential,
                ["Cable"] = CableName,
                ["Destination"] = Destination,
                ["Remarks"] = Remarks ?? string.Empty
            };
        }
    }
}
