using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenarSoft.Models
{
    /// Интерфейс для объектов, которые могут генерировать имя страницы в EPLAN
    public interface IHasPageName
    {
        string GetPageName();
    }
}
