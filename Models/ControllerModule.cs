using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenarSoft.Models
{
    /// Модуль контроллера (ввода-вывода)
    public class ControllerModule
    {
        /// Тег модуля (например, 1A5)
        public string Tag { get; set; } = string.Empty;

        /// Тип модуля
        public string Type { get; set; } = string.Empty;

        /// Номер канала
        public int Channel { get; set; }

        /// Описание модуля
        public string? Description { get; set; }

        /// Артикул модуля (для поиска в EPLAN)
        public string? ArticleNumber { get; set; }
    }
}
