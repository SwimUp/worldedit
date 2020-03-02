using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using Verse;

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

        public static void CreateWorldObject(WorldObjectDef def, int tile, Faction faction = null)
        {
            WorldObject worldObject = WorldObjectMaker.MakeWorldObject(def);
            worldObject.Tile = tile;
            worldObject.SetFaction(faction);
            Find.WorldObjects.Add(worldObject);
        }

        public static T Deserialize<T>(XmlSerializer serializer, string path) where T : class
        {
            using (StreamReader reader = new StreamReader(path))
            {
                T @class = serializer.Deserialize(reader) as T;

                return @class;
            }
        }

        public static List<TItem> CreateList<TItem>()
        {
            Type listType = GetGenericListType<TItem>();
            List<TItem> list = (List<TItem>)Activator.CreateInstance(listType);
            return list;
        }

        public static Type GetGenericListType<TItem>()
        {
            Type objTyp = typeof(TItem);
            var defaultListType = typeof(List<>);
            Type[] itemTypes = { objTyp };
            Type listType = defaultListType.MakeGenericType(itemTypes);
            return listType;
        }

        public static object GetValue(object value, Type type)
        {
            return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value.ToString());
        }

    }

    public class ConfirmActionPage : Page
    {
        public override Vector2 InitialSize => new Vector2(200, 125);
        public override string PageTitle => "Confirm";

        private Action confirmAction;
        private Action declineAction;

        public ConfirmActionPage(Action confirm, Action decline = null)
        {
            confirmAction = confirm;
            declineAction = decline;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(0, 0, 190, 20), Translator.Translate("ConfirmActionWindow"));

            if(Widgets.ButtonText(new Rect(0, 30, 180, 20), Translator.Translate("ConfirmAction")))
            {
                confirmAction.Invoke();
            }

            if (Widgets.ButtonText(new Rect(0, 60, 180, 20), Translator.Translate("DeclineAction")))
            {
                if (declineAction != null)
                    declineAction.Invoke();
                else
                    Close();
            }
        }
    }
}
