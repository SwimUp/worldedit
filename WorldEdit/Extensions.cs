using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldEdit
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            if(action == null)
            {
                throw new ArgumentNullException("Action cannot be null");
            }

            foreach(T value in values)
            {
                action(value);
            }
        }
    }
}
