using Eplan.EplApi.Base;
using Eplan.EplApi.Gui;
using LenarSoft.EplanActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// Класс регистрации (AddIn)
namespace LenarSoft
{
    public class AddInModule : Eplan.EplApi.ApplicationFramework.IEplAddIn
    {
        public bool OnRegister(ref System.Boolean bLoadOnStart)
        {
            var ribbonBar = new Eplan.EplApi.Gui.RibbonBar();

            ribbonBar.AddCommand("Генератор Шкаф ПВУ", "PVU_GeneratorAction");
            ribbonBar.AddCommand("DisplayTheTypeAction", "DisplayTheTypeAction");
            //ribbonBar.AddCommand("InsertMacro", "InsertMacroAction");
            //ribbonBar.AddCommand("Устройства на дин рейке", "MarkViewPortAction");
            //ribbonBar.AddCommand("Заменить шкаф в обзоре модели", "ReplaceCabinetInViewportAction");
            //ribbonBar.AddCommand("Показать только дверь в обзоре модели", "ShowOnlyDoorInViewportAction");
            //ribbonBar.AddCommand("Вставить клеммы", "InsertTerminalBlockAction");

            bLoadOnStart = true;
            return true;
        }
        public bool OnUnregister()
        {
            return true;
        }
        public bool OnInit()
        {
            return true;
        }
        public bool OnInitGui()
        {
            return true;
        }
        public bool OnExit()
        {
            var ribbonBar = new Eplan.EplApi.Gui.RibbonBar();
            ribbonBar.RemoveCommand("PVU_GeneratorAction");
            ribbonBar.RemoveCommand("DisplayTheTypeAction");
            ribbonBar.RemoveCommand("InsertMacroAction");
            //ribbonBar.RemoveCommand("MarkViewPortAction");
            //ribbonBar.RemoveCommand("ReplaceCabinetInViewportAction");
            //ribbonBar.RemoveCommand("ShowOnlyDoorInViewportAction");
            ribbonBar.RemoveCommand("InsertTerminalBlockAction");
            return true;
        }
    }
}