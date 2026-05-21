using System;
using System.IO;
using System.Diagnostics;
using System.Timers;
using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.DataModel;

namespace LenarSoft.EplanActions
{
    public class ActiveTimeTrackerAction : IEplAddIn
    {
        private static readonly Stopwatch _stopwatch = new Stopwatch();
        private static string _currentProjectName = string.Empty;
        private static DateTime _sessionStartTime;
        private static System.Timers.Timer _statusCheckTimer; // Используем System.Timers.Timer вместо Forms.Timer
        private static readonly string _baseDirectory = @"C:\EPLAN_TimeTracker";
        private static string _logFilePath;

        public bool OnRegister(ref bool bLoadOnStart)
        {
            File.WriteAllText(@"C:\TEST_EPLAN_PLUGIN_REGISTER.txt",
                $"OnRegister called at {DateTime.Now}{Environment.NewLine}");
            bLoadOnStart = true;
            return true;
        }

        public bool OnUnregister()
        {
            StopTimer();
            return true;
        }

        public bool OnInit()
        {
            try
            {
                if (!Directory.Exists(_baseDirectory))
                {
                    Directory.CreateDirectory(_baseDirectory);
                }

                _logFilePath = Path.Combine(_baseDirectory, $"ProjectTimeLog_{DateTime.Now:yyyy-MM}.txt");

                WriteToLog("OnInit: Базовая инициализация пройдена.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Критическая ошибка создания директории: " + ex.Message);
                WriteToLog($"Ошибка в OnInit: {ex.Message}");
            }

            return true;
        }

        public bool OnInitGui()
        {
            File.WriteAllText(@"C:\EPLAN_TimeTracker_test.txt", $"Started at {DateTime.Now}");
            try
            {
                if (_statusCheckTimer == null)
                {
                    _statusCheckTimer = new System.Timers.Timer(5000);
                    _statusCheckTimer.Elapsed += OnStatusCheckTimer;
                    _statusCheckTimer.AutoReset = true;
                    _statusCheckTimer.SynchronizingObject = null; // Работает в фоновом потоке
                    _statusCheckTimer.Start();

                    WriteToLog("OnInitGui: Таймер успешно запущен (System.Timers.Timer)");

                    // Первая проверка через 1 секунду
                    var startupTimer = new System.Timers.Timer(1000);
                    startupTimer.Elapsed += (s, e) =>
                    {
                        CheckProjectStatus();
                        startupTimer.Stop();
                        startupTimer.Dispose();
                    };
                    startupTimer.AutoReset = false;
                    startupTimer.Start();
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"Ошибка запуска таймера в OnInitGui: {ex.Message}");
            }

            return true;
        }

        public bool OnExit()
        {
            SaveTimeToFile();
            StopTimer();
            WriteToLog("OnExit: Плагин завершил работу.");
            return true;
        }

        private void StopTimer()
        {
            if (_statusCheckTimer != null)
            {
                _statusCheckTimer.Stop();
                _statusCheckTimer.Elapsed -= OnStatusCheckTimer;
                _statusCheckTimer.Dispose();
                _statusCheckTimer = null;
            }
        }

        private void OnStatusCheckTimer(object sender, ElapsedEventArgs e)
        {
            CheckProjectStatus();
        }

        private void CheckProjectStatus()
        {
            try
            {
                ProjectManager projectManager = new ProjectManager();
                var allProjects = projectManager.OpenProjects;
                string activeProjectName = string.Empty;

                if (allProjects != null && allProjects.Length > 0)
                {
                    foreach (Project project in allProjects)
                    {
                        if (project != null && !string.IsNullOrEmpty(project.ProjectName))
                        {
                            activeProjectName = project.ProjectName;
                            WriteToLog($"Обнаружен проект: {activeProjectName}");
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(activeProjectName))
                {
                    if (!_stopwatch.IsRunning)
                    {
                        _currentProjectName = activeProjectName;
                        _sessionStartTime = DateTime.Now;
                        _stopwatch.Start();
                        WriteToLog($">>> НАЧАЛО ОТСЛЕЖИВАНИЯ для проекта: {_currentProjectName}");
                        WriteToMainLog($"НАЧАЛО: {_sessionStartTime:yyyy-MM-dd HH:mm:ss} | Проект: {_currentProjectName}");
                    }
                    else if (_currentProjectName != activeProjectName)
                    {
                        WriteToLog($"Смена проекта: {_currentProjectName} -> {activeProjectName}");
                        SaveTimeToFile();

                        _currentProjectName = activeProjectName;
                        _sessionStartTime = DateTime.Now;
                        _stopwatch.Restart();
                        WriteToMainLog($"СМЕНА: {_sessionStartTime:yyyy-MM-dd HH:mm:ss} | Проект: {_currentProjectName}");
                    }
                }
                else if (_stopwatch.IsRunning)
                {
                    WriteToLog(">>> ПРОЕКТ ЗАКРЫТ, отслеживание остановлено");
                    SaveTimeToFile();
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"Ошибка проверки статуса: {ex.Message}");
            }
        }

        private void WriteToMainLog(string message)
        {
            try
            {
                File.AppendAllText(_logFilePath, $"{message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"Ошибка записи в main log: {ex.Message}");
            }
        }

        private void SaveTimeToFile()
        {
            if (_stopwatch.IsRunning && !string.IsNullOrEmpty(_currentProjectName))
            {
                _stopwatch.Stop();
                TimeSpan elapsed = _stopwatch.Elapsed;

                if (elapsed.TotalSeconds >= 1) // Игнорируем очень короткие сессии
                {
                    string formattedTime = $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
                    double minutesSpent = Math.Round(elapsed.TotalMinutes, 2);

                    try
                    {
                        string logLine = $"Период: {_sessionStartTime:yyyy-MM-dd HH:mm:ss} - {DateTime.Now:HH:mm:ss} | " +
                                        $"Проект: {_currentProjectName} | " +
                                        $"Время: {formattedTime} ({minutesSpent} мин.)";

                        File.AppendAllText(_logFilePath, logLine + Environment.NewLine);
                        WriteToLog($"СОХРАНЕНО: {formattedTime} для {_currentProjectName}");
                    }
                    catch (Exception ex)
                    {
                        WriteToErrorLog($"Ошибка записи в лог времени: {ex.Message}");
                    }
                }

                _currentProjectName = string.Empty;
            }
        }

        private void WriteToLog(string message)
        {
            try
            {
                if (!Directory.Exists(_baseDirectory))
                    Directory.CreateDirectory(_baseDirectory);

                string logPath = Path.Combine(_baseDirectory, "debug_log.txt");
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch { }
        }

        private void WriteToErrorLog(string message)
        {
            try
            {
                if (!Directory.Exists(_baseDirectory))
                    Directory.CreateDirectory(_baseDirectory);

                string errorPath = Path.Combine(_baseDirectory, "error_log.txt");
                File.AppendAllText(errorPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}{Environment.NewLine}");
            }
            catch { }
        }
    }
}