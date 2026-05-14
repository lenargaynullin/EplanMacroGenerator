using Eplan.EplApi.DataModel.EObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenarSoft.Models
{
    /// Клеммник (ряд клемм)
    public class TerminalBlock
    {
        /// Обозначение клеммника (например, XT101)
        public string Name { get; set; } = string.Empty;

        /// Тип клемм (например, UK 3 N)
        public string Type { get; set; } = string.Empty;

        /// Список клемм в клеммнике
        public List<Terminal> Terminals { get; set; } = new();

        /// Получить полный DT для клеммы
        public string GetTerminalDT(Terminal terminal)
            => $"+{Name}:{terminal.Number}";
    }
}
