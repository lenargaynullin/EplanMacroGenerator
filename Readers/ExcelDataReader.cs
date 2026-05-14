using OfficeOpenXml;
using LenarSoft.Models;
using LenarSoft.Extensions;  // ← обязательно добавьте, если используете метод расширения
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LenarSoft.Readers
{
    /// <summary>
    /// Чтение данных из Excel-файла
    /// </summary>
    public class ExcelDataReader
    {
        private readonly string _filePath;

        public ExcelDataReader(string filePath)
        {
            _filePath = filePath;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Основной метод - читает все листы и возвращает структуру проекта
        /// </summary>
        public ProjectData ReadAll()
        {
            var data = new ProjectData();

            using var package = new ExcelPackage(new FileInfo(_filePath));

            // Читаем каждый лист
            data.ControlCables = ReadControlCables(package).ToList();
            data.NetworkConnections = ReadNetworkConnections(package).ToList();
            data.PowerCables = ReadPowerCables(package).ToList();

            // Группируем по шкафам для удобства генерации
            GroupByCabinets(data);

            return data;
        }

        private IEnumerable<ControlCable> ReadControlCables(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["Физика"];
            if (sheet == null) yield break;

            int row = 3; // Начинаем с 3-й строки (после заголовков)

            while (sheet.Cells[row, 1].Text != string.Empty)
            {
                var cable = new ControlCable
                {
                    Number = ParseInt(sheet.Cells[row, 1].Text),
                    LocationTitle = sheet.Cells[row, 2].Text,
                    SignalName = sheet.Cells[row, 3].Text,
                    SignalTag = sheet.Cells[row, 4].Text,
                    SignalType = sheet.Cells[row, 5].Text,
                    SignalChar = sheet.Cells[row, 6].Text,
                    Safety = sheet.Cells[row, 7].Text,
                    Hart = sheet.Cells[row, 9].Text,
                    SchemeCode = sheet.Cells[row, 11].Text,
                    SensorTag = sheet.Cells[row, 12].Text,
                    CableNumber = sheet.Cells[row, 13].Text,
                    CableType = sheet.Cells[row, 14].Text,
                    CoreNumbers = sheet.Cells[row, 15].Text,
                    CabinetLocation = sheet.Cells[row, 16].Text,
                    CabinetTag = sheet.Cells[row, 17].Text,
                    TerminalBlock = sheet.Cells[row, 18].Text,
                    Terminals = sheet.Cells[row, 19].Text,
                    FeTerminalBlock = sheet.Cells[row, 20].Text,
                    FeTerminals = sheet.Cells[row, 21].Text,
                    BizTag = sheet.Cells[row, 22].Text,
                    BizContacts = sheet.Cells[row, 23].Text,
                    DeviceTag = sheet.Cells[row, 24].Text,
                    DeviceContacts = sheet.Cells[row, 25].Text,
                    ControllerModule = sheet.Cells[row, 26].Text,
                    ControllerCabinet = sheet.Cells[row, 28].Text,
                    ControllerSignalTag = sheet.Cells[row, 29].Text,
                    Units = sheet.Cells[row, 30].Text,
                    Scale = sheet.Cells[row, 31].Text
                };

                // Парсим номер канала (если есть)
                if (int.TryParse(sheet.Cells[row, 27].Text, out int channel))
                {
                    cable.ModuleChannel = channel;
                }

                // Парсим длину кабеля (если есть)
                if (double.TryParse(sheet.Cells[row, 33].Text, out double length))
                {
                    cable.CableLength = length;
                }

                yield return cable;
                row++;
            }
        }

        private IEnumerable<NetworkConnection> ReadNetworkConnections(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["Интерфейсы"];
            if (sheet == null) yield break;

            int row = 3;

            while (sheet.Cells[row, 1].Text != string.Empty)
            {
                var conn = new NetworkConnection
                {
                    Number = ParseInt(sheet.Cells[row, 1].Text),
                    Description = sheet.Cells[row, 2].Text,
                    SourceLocation = sheet.Cells[row, 3].Text,
                    SourceEquipment = sheet.Cells[row, 4].Text,
                    SourceDevice = sheet.Cells[row, 5].Text,
                    SourceContacts = sheet.Cells[row, 6].Text,
                    ConnectionType = sheet.Cells[row, 7].Text,
                    CableNumber = sheet.Cells[row, 8].Text,
                    CableType = sheet.Cells[row, 9].Text,
                    CoreNumbers = sheet.Cells[row, 10].Text,
                    DestLocation = sheet.Cells[row, 11].Text,
                    DestEquipment = sheet.Cells[row, 12].Text,
                    DestDevice = sheet.Cells[row, 13].Text,
                    DestContacts = sheet.Cells[row, 14].Text,
                    Spec = sheet.Cells[row, 16].Text,
                    CableTag = sheet.Cells[row, 17].Text
                };

                // Парсим длину
                if (double.TryParse(sheet.Cells[row, 15].Text, out double length))
                {
                    conn.CableLength = length;
                }

                yield return conn;
                row++;
            }
        }

        private IEnumerable<PowerCable> ReadPowerCables(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["Питание"];
            if (sheet == null) yield break;

            int row = 3;

            while (sheet.Cells[row, 1].Text != string.Empty)
            {
                var power = new PowerCable
                {
                    Number = ParseInt(sheet.Cells[row, 1].Text),
                    Description = sheet.Cells[row, 2].Text,
                    SourceLocation = sheet.Cells[row, 3].Text,
                    SourceEquipment = sheet.Cells[row, 4].Text,
                    SourceTerminalBlock = sheet.Cells[row, 5].Text,
                    SourceTerminals = sheet.Cells[row, 6].Text,
                    Voltage = sheet.Cells[row, 7].Text,
                    CableNumber = sheet.Cells[row, 8].Text,
                    CableType = sheet.Cells[row, 9].Text,
                    DestLocation = sheet.Cells[row, 10].Text,
                    DestEquipment = sheet.Cells[row, 11].Text,
                    DestTerminalBlock = sheet.Cells[row, 12].Text,
                    DestTerminals = sheet.Cells[row, 13].Text
                };

                // Парсим длину
                if (double.TryParse(sheet.Cells[row, 14].Text, out double length))
                {
                    power.CableLength = length;
                }

                yield return power;
                row++;
            }
        }

        private void GroupByCabinets(ProjectData data)
        {
            foreach (var cable in data.ControlCables)
            {
                if (string.IsNullOrEmpty(cable.CabinetLocation)) continue;

                // Получаем или создаем шкаф
                var cabinet = data.Cabinets.GetOrAdd(cable.CabinetLocation,
                    () => new CabinetData
                    {
                        Title = cable.CabinetLocation,
                        Tag = cable.CabinetTag
                    });

                // Получаем или создаем клеммник
                var terminalBlock = cabinet.TerminalBlocks
                    .FirstOrDefault(t => t.Name == cable.TerminalBlock);

                if (terminalBlock == null)
                {
                    terminalBlock = new TerminalBlock
                    {
                        Name = cable.TerminalBlock,
                        Type = GetTerminalBlockType(cable.SignalType)
                    };
                    cabinet.TerminalBlocks.Add(terminalBlock);
                }

                // Добавляем клемму
                terminalBlock.Terminals.Add(new Terminal
                {
                    Number = ParseTerminalNumber(cable.Terminals),
                    Potential = GetPotentialFromSignal(cable.SignalChar),
                    CableName = cable.CableNumber,
                    Destination = cable.SignalName,
                    Remarks = cable.SignalTag
                });
            }

            // Сортируем клеммы в каждом клеммнике по номеру
            foreach (var cabinet in data.Cabinets.Values)
            {
                foreach (var block in cabinet.TerminalBlocks)
                {
                    block.Terminals = block.Terminals
                        .OrderBy(t => t.Number)
                        .ToList();
                }
            }
        }

        // Вспомогательные методы
        private int ParseInt(string text)
            => int.TryParse(text, out int value) ? value : 0;

        private int ParseTerminalNumber(string terminals)
        {
            if (string.IsNullOrEmpty(terminals)) return 0;

            // Берем первое число из "3,4" или "3.4" или "3"
            var parts = terminals.Split(',', '.');
            return ParseInt(parts[0].Trim());
        }

        private string GetPotentialFromSignal(string signalChar)
        {
            if (signalChar.Contains("4-20mA")) return "+24V";
            if (signalChar.Contains("0-10V")) return "+24V";
            if (signalChar.Contains("DI")) return "+24V";
            if (signalChar.Contains("DO")) return "+24V";
            return "0V";
        }

        private string GetTerminalBlockType(string signalType)
        {
            return signalType switch
            {
                "AI" => "UK 3 N",
                "AO" => "UK 3 N",
                "DI" => "UK 3 N",
                "DO" => "UK 5 N",
                "PI" => "UK 3 N",
                _ => "UK 3 N"
            };
        }
    }
}