using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WorldEdit
{
    /// <summary>
    /// Утилиты
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Получает поле и возвращает его значение
        /// </summary>
        /// <param name="type">Тип, где содержится поле</param>
        /// <param name="instance">Объект у которого его нужно получить</param>
        /// <param name="fieldName">Имя поля</param>
        /// <returns></returns>
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        /// <summary>
        /// Получает свойство и возвращает его значение
        /// </summary>
        /// <param name="type">Тип, где содержится свойство</param>
        /// <param name="instance">Объект у которого его нужно получить</param>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns></returns>
        public static object GetInstanceProperty(Type type, object instance, string propertyName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            PropertyInfo property = type.GetProperty(propertyName, bindFlags);
            return property.GetValue(instance, null);
        }
    }
}
