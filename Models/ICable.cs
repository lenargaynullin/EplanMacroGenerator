using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenarSoft.Models
{
    /// Общий интерфейс для всех типов кабелей
    public interface ICable
    {
        string CableNumber { get; }
        string CableType { get; }
        double CableLength { get; }
        string GetPageName();
    }
}
