using Eplan.EplApi.DataModel;
using Eplan.EplApi.DataModel.E3D;  // для 3D объектов
using Eplan.EplApi.DataModel.Graphics;
using Eplan.EplApi.Base;
using System.Collections.Generic;
using System.Linq;

namespace LenarSoft.Generators
{
    /// Генератор видовых экранов (ViewPlacement) для 3D-видов
    /// </summary>
    public class ViewportGenerator
    {
        private readonly Project _project;

        public ViewportGenerator(Project project)
        {
            _project = project;
        }

        /// <summary>
        /// Создать видовой экран для монтажного пространства
        /// </summary>
        public ViewPlacement CreateViewport(
            Page targetPage,                    // страница для размещения вида
            InstallationSpace installationSpace, // монтажное пространство
            string viewName,                     // имя вида
            RectangleD area,                     // область на странице
            ViewOrientation orientation = ViewOrientation.Isometric) // ориентация
        {
            // Создаем видовой экран
            var viewport = new ViewPlacement();
            viewport.Create(_project, installationSpace);

            // Привязываем к странице
            viewport.Page = targetPage;

            // Устанавливаем область отображения на странице (в мм)
            viewport.Area = area;

            // Устанавливаем корневые элементы (что показываем)
            viewport.RootElements = new Placement3D[] { installationSpace };

            // Устанавливаем имя
            viewport.Properties.VIEW_NAME = viewName;

            // Устанавливаем ориентацию
            SetViewOrientation(viewport, orientation);

            // Обновляем видовой экран
            viewport.Update();

            return viewport;
        }

        /// <summary>
        /// Создать видовой экран для конкретного шкафа
        /// </summary>
        public ViewPlacement CreateCabinetViewport(
            Page targetPage,
            Cabinet cabinet,
            string viewName,
            RectangleD area,
            ViewOrientation orientation = ViewOrientation.Isometric)
        {
            var viewport = new ViewPlacement();
            viewport.Create(_project, cabinet.Parent as InstallationSpace ?? cabinet);

            viewport.Page = targetPage;
            viewport.Area = area;
            viewport.RootElements = new Placement3D[] { cabinet };
            viewport.Properties.VIEW_NAME = viewName;

            SetViewOrientation(viewport, orientation);
            viewport.Update();

            return viewport;
        }

        /// <summary>
        /// Создать несколько видов на одной странице
        /// </summary>
        public List<ViewPlacement> CreateMultiViewPage(
            Page page,
            InstallationSpace installationSpace,
            string baseName)
        {
            var viewports = new List<ViewPlacement>();

            // Сетка 2x2 видов
            double pageWidth = 297; // A4 ширина
            double pageHeight = 210; // A4 высота
            double margin = 10;
            double viewWidth = (pageWidth - 3 * margin) / 2;
            double viewHeight = (pageHeight - 3 * margin) / 2;

            // Изометрия (слева сверху)
            viewports.Add(CreateViewport(
                page, installationSpace, $"{baseName}_ISO",
                new RectangleD(
                    new PointD(margin, pageHeight - margin - viewHeight),
                    new PointD(margin + viewWidth, pageHeight - margin)
                ),
                ViewOrientation.Isometric
            ));

            // Вид спереди (справа сверху)
            viewports.Add(CreateViewport(
                page, installationSpace, $"{baseName}_Front",
                new RectangleD(
                    new PointD(margin + viewWidth + margin, pageHeight - margin - viewHeight),
                    new PointD(margin + 2 * viewWidth + margin, pageHeight - margin)
                ),
                ViewOrientation.Front
            ));

            // Вид сверху (слева снизу)
            viewports.Add(CreateViewport(
                page, installationSpace, $"{baseName}_Top",
                new RectangleD(
                    new PointD(margin, margin),
                    new PointD(margin + viewWidth, margin + viewHeight)
                ),
                ViewOrientation.Top
            ));

            // Вид сбоку (справа снизу)
            viewports.Add(CreateViewport(
                page, installationSpace, $"{baseName}_Side",
                new RectangleD(
                    new PointD(margin + viewWidth + margin, margin),
                    new PointD(margin + 2 * viewWidth + margin, margin + viewHeight)
                ),
                ViewOrientation.Side
            ));

            return viewports;
        }

        /// <summary>
        /// Установка ориентации вида
        /// </summary>
        private void SetViewOrientation(ViewPlacement viewport, ViewOrientation orientation)
        {
            switch (orientation)
            {
                case ViewOrientation.Isometric:
                    // Изометрическая проекция
                    viewport.Properties.VIEW_REPRESENTATION_TYPE = 1; // Изометрия
                    viewport.Properties.VIEW_ANGLE1 = 45.0;
                    viewport.Properties.VIEW_ANGLE2 = 35.264;
                    break;

                case ViewOrientation.Front:
                    // Вид спереди
                    viewport.Properties.VIEW_REPRESENTATION_TYPE = 2; // Ортогональная
                    viewport.Properties.VIEW_ANGLE1 = 0.0;
                    viewport.Properties.VIEW_ANGLE2 = 0.0;
                    break;

                case ViewOrientation.Top:
                    // Вид сверху
                    viewport.Properties.VIEW_REPRESENTATION_TYPE = 2;
                    viewport.Properties.VIEW_ANGLE1 = 90.0;
                    viewport.Properties.VIEW_ANGLE2 = 0.0;
                    break;

                case ViewOrientation.Side:
                    // Вид сбоку
                    viewport.Properties.VIEW_REPRESENTATION_TYPE = 2;
                    viewport.Properties.VIEW_ANGLE1 = 0.0;
                    viewport.Properties.VIEW_ANGLE2 = 90.0;
                    break;
            }
        }

        /// <summary>
        /// Настройка отображения видового экрана
        /// </summary>
        public void ConfigureViewport(ViewPlacement viewport, ViewportOptions options)
        {
            // Показывать ли соединения
            viewport.Properties.VIEW_SHOW_CONNECTIONS = options.ShowConnections ? 1 : 0;

            // Показывать ли обозначения устройств
            viewport.Properties.VIEW_SHOW_DEVICE_TAGS = options.ShowDeviceTags ? 1 : 0;

            // Показывать ли монтажные рельсы
            viewport.Properties.VIEW_SHOW_MOUNTINGRAILS = options.ShowMountingRails ? 1 : 0;

            // Показывать ли кабельные каналы
            viewport.Properties.VIEW_SHOW_CABLEDUCTS = options.ShowCableDucts ? 1 : 0;

            // Масштаб
            if (options.Scale > 0)
            {
                viewport.Properties.VIEW_SCALE = options.Scale;
            }

            viewport.Update();
        }
    }

    /// <summary>
    /// Опции отображения видового экрана
    /// </summary>
    public class ViewportOptions
    {
        public bool ShowConnections { get; set; } = true;
        public bool ShowDeviceTags { get; set; } = true;
        public bool ShowMountingRails { get; set; } = true;
        public bool ShowCableDucts { get; set; } = true;
        public double Scale { get; set; } = 1.0;
    }

    /// <summary>
    /// Ориентация вида
    /// </summary>
    public enum ViewOrientation
    {
        Isometric,
        Front,
        Top,
        Side,
        Back,
        Bottom
    }
}
