using System;
using System.Windows.Forms;

namespace TricksGame
{
    public class Program
    {
        // --- Конфигурация ---
        // Единый источник версии для отображения в меню
        public const string GameVersion = "Alpha 0.0.1";

        [STAThread]
        public static void Main()
        {
            // 1. Инициализация графики и DPI (важно для четкости на современных экранах)
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

            // 2. Стабильность: Глобальная обработка ошибок (чтобы игра не падала молча)
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += UIThreadExceptionHandler;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // 3. Визуал: Стандартные настройки WinForms
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false); // Используем GDI+ для шрифтов

            // --- Запуск ---
            Application.Run(new MainForm());
        }

        // Обработчик ошибок в UI-потоке
        private static void UIThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"КРИТИЧЕСКАЯ ОШИБКА:\n{e.Exception.Message}", "FATAL ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        // Обработчик фоновых ошибок
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // В релизе здесь можно добавить логирование в файл
            Application.Exit();
        }
    }
}