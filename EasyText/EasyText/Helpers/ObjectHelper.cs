using System;
using System.Collections.Generic;

namespace EasyText.Helpers
{
	internal class ObjectHelper
	{
		private static readonly Dictionary<Type, object> objects = new Dictionary<Type, object>();

		public static T GetObject<T>() where T : class
		{
			var type = typeof (T);
			if (!objects.ContainsKey(type))
			{
				throw new ArgumentException("Can't find type");
			}

			return objects[typeof (T)] as T;
		}

		public static void RegisterObject(Type type, object obj)
		{
			objects[type] = obj;
		}
	}
}