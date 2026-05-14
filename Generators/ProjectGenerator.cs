using Eplan.EplApi.DataModel;
using Eplan.EplApi.Base;
using System;
using System.Linq;
using LenarSoft.Generators;  // для PageGenerator, TerminalGenerator, MacroGenerator
using LenarSoft.Models;       // для моделей данных
using LenarSoft.Readers;       // если используется
using LenarSoft.Extensions;    // для расширений

namespace LenarSoft.Generators
{
    /// <summary>
    /// Главный генератор проекта - собирает всё вместе
    /// </summary>
    public class ProjectGenerator
    {
        private readonly Project _project;
        private readonly PageGenerator _pageGen;
        private readonly TerminalGenerator _terminalGen;
        private readonly MacroGenerator _macroGen;

        public ProjectGenerator(Project project)
        {
            _project = project;
            _pageGen = new PageGenerator(project);
            _terminalGen = new TerminalGenerator(project);
            _macroGen = new MacroGenerator(project);
        }

        /// <summary>
        /// Генерировать полный проект из данных Excel
        /// </summary>
        public void GenerateFromData(ProjectData data)
        {
            using (var transaction = _project.CreateTransaction())
            {
                try
                {
                    // 1. Генерируем страницы для каждого шкафа
                    GenerateCabinets(data);

                    // 2. Генерируем общие страницы сетевых подключений
                    GenerateNetworks(data);

                    // 3. Генерируем страницы питания
                    GeneratePower(data);

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void GenerateCabinets(ProjectData data)
        {
            foreach (var cabinet in data.Cabinets.Values)
            {
                // Главная страница шкафа
                var mainPage = _pageGen.CreateCabinetMainPage(cabinet);

                // Добавляем информацию о шкафе
                AddCabinetInfo(mainPage, cabinet);

                // Страницы клеммников
                int pageNumber = 2;
                foreach (var block in cabinet.TerminalBlocks)
                {
                    if (block.Terminals.Count == 0) continue;

                    var blockPage = _pageGen.CreateTerminalBlockPage(cabinet, block, pageNumber);
                    _terminalGen.GenerateTerminalBlock(blockPage, block, cabinet.Title);

                    pageNumber++;
                }

                // Группируем сигналы по устройствам для главной страницы
                var signals = data.ControlCables
                    .Where(c => c.CabinetLocation == cabinet.Title)
                    .GroupBy(c => c.DeviceTag);

                int yPos = 50;
                foreach (var deviceGroup in signals)
                {
                    if (string.IsNullOrEmpty(deviceGroup.Key)) continue;

                    var deviceData = new System.Collections.Generic.Dictionary<string, string>
                    {
                        ["DeviceTag"] = deviceGroup.Key,
                        ["SignalsCount"] = deviceGroup.Count().ToString()
                    };

                    _macroGen.InsertMacroWithPlaceholders(
                        mainPage,
                        @"$(MD_MACROS)\Devices\FTA.ema",
                        deviceData,
                        new PointD(50, yPos, 0)
                    );

                    yPos += 40;
                }
            }
        }

        private void GenerateNetworks(ProjectData data)
        {
            // Группируем по типу подключения
            var systemConns = data.NetworkConnections.Where(c => c.ConnectionType == "SYSTEM").ToList();
            var foConns = data.NetworkConnections.Where(c => c.ConnectionType == "FO").ToList();
            var tpConns = data.NetworkConnections.Where(c => c.ConnectionType == "TP").ToList();

            if (systemConns.Any())
            {
                var page = _pageGen.CreateNetworkPage("M0-85-050", "SYSTEM");
                GenerateConnectionTable(page, systemConns);
            }

            if (foConns.Any())
            {
                var page = _pageGen.CreateNetworkPage("M0-85-050", "FO");
                GenerateConnectionTable(page, foConns);
            }

            if (tpConns.Any())
            {
                var page = _pageGen.CreateNetworkPage("M0-85-050", "TP");
                GenerateConnectionTable(page, tpConns);
            }
        }

        private void GenerateConnectionTable(Page page, System.Collections.Generic.List<NetworkConnection> connections)
        {
            int yPos = 50;
            foreach (var conn in connections)
            {
                _macroGen.InsertMacro(page, conn, new PointD(50, yPos, 0));
                yPos += 20;
            }
        }

        private void GeneratePower(ProjectData data)
        {
            if (!data.PowerCables.Any()) return;

            var page = _pageGen.CreatePowerPage("M0-85-050");

            int yPos = 50;
            foreach (var power in data.PowerCables)
            {
                _macroGen.InsertMacro(page, power, new PointD(50, yPos, 0));
                yPos += 20;
            }
        }

        private void AddCabinetInfo(Page page, CabinetData cabinet)
        {
            // Добавляем текстовую информацию о шкафе
            var text = new Text()
            {
                TextString = $"Шкаф: {cabinet.Title}\nТег: {cabinet.Tag}\nКлеммников: {cabinet.TerminalBlocks.Count}"
            };
            text.Create(page, new PointD(200, 20, 0));
        }
    }
}