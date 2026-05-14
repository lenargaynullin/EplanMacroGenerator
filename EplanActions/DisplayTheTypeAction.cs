using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.Base.Enums;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.DataModel.E3D;
using Eplan.EplApi.HEServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

// Класс Action (Точка входа)
namespace LenarSoft.EplanActions
{
    public class DisplayTheTypeAction : IEplAction
    {
        public bool OnRegister(ref string Name, ref int Priority)
        {
            Name = "DisplayTheTypeAction"; // Имя экшена
            Priority = 0;                 // Приоритет (0 — стандарт)
            return true;
        }
        public bool Execute(ActionCallingContext ctx)
        {

            // 1. Вызываем логику импорта
            SelectionSet Set = new SelectionSet();
            if (Set.Selection.Count() == 0)
            {
                return false;
            }
            foreach (StorableObject selObj in Set.Selection)
            {

                String name = selObj.GetTypeName();
                String  identifier = selObj.ToStringIdentifier();

                MessageBox.Show("StorableObject = " + selObj.ToString());
                MessageBox.Show("GetTypeName() = " + name);
                MessageBox.Show("ToStringIdentifier() = " + identifier);


            }
            return true;
        }
        private string GetDeviceInfo(Component component)
        {
            var funcDef = component.FunctionDefinition;
            if (funcDef == null) return "Неизвестное устройство";

            return $@"
        Определение: {funcDef.Name}
        Категория: {funcDef.FunctionCategory}
        Группа: {funcDef.GroupName}
        ID: {funcDef.Id}
        Главная функция: {funcDef.IsMainFunction}
        Сетевой соединитель: {funcDef.IsNetConnecting}
        Безопасность: {funcDef.IsSafetyRelevant}
        ";
        }
        public bool OnConfig(ref string Name, ref string Function) => true;
        public void GetActionProperties(ref ActionProperties props) { }
    }

}
