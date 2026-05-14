using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenarSoft.Models
{
    /// Данные по шкафу автоматизации
    public class CabinetData
    {
        /// Титул расположения (например, M0-85-050)
        public string Title { get; set; } = string.Empty;

        /// Полный тег шкафа
        public string Tag { get; set; } = string.Empty;

        /// Клеммники в шкафу
        public List<TerminalBlock> TerminalBlocks { get; set; } = new();

        /// Модули контроллера в шкафу
        public List<ControllerModule> Modules { get; set; } = new();

        /// Получить имя основной страницы для шкафа
        public string GetMainPageName() => $"={Title}/1.1";

        /// Получить имя страницы для клеммника
        public string GetTerminalBlockPageName(TerminalBlock block, int pageNumber)
            => $"={Title}+{block.Name}/{pageNumber}.1";
    }
}
