// Файл: Helpers/TimeHelper.cs
using System;

namespace MajorAuthor.Helpers
{
    /// <summary>
    /// Статический класс-помощник для работы с датами и временем.
    /// Отделение этой логики от контроллера соответствует принципу единственной ответственности.
    /// </summary>
    public static class TimeHelper
    {
        /// <summary>
        /// Вспомогательная функция для получения относительного времени (например, "2 дня назад").
        /// </summary>
        /// <param name="dateTime">Дата и время для форматирования.</param>
        /// <returns>Строка, представляющая относительное время.</returns>
        public static string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan <= TimeSpan.FromSeconds(60))
                return $"{timeSpan.Seconds} секунд назад";
            if (timeSpan <= TimeSpan.FromMinutes(60))
                return $"{timeSpan.Minutes} минут назад";
            if (timeSpan <= TimeSpan.FromHours(24))
                return $"{timeSpan.Hours} часов назад";
            if (timeSpan <= TimeSpan.FromDays(30))
                return $"{timeSpan.Days} дней назад";
            if (timeSpan <= TimeSpan.FromDays(365))
                return $"{timeSpan.Days / 30} месяцев назад";
            return $"{timeSpan.Days / 365} лет назад";
        }
    }
}
