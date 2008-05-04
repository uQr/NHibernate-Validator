using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NHibernate.Validator.Exceptions;

namespace NHibernate.Validator.Util
{
	/// <summary>
	/// Utils methods for type issues.
	/// </summary>
	public sealed class TypeUtils
	{
		public static BindingFlags AnyVisibilityInstance = BindingFlags.Instance | BindingFlags.Public |
																									 BindingFlags.NonPublic | BindingFlags.Static;

		/// <summary>
		/// Get the Generic Arguments of a <see cref="IDictionary{TKey,TValue}"/>
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static KeyValuePair<System.Type, System.Type> GetGenericTypesOfDictionary(MemberInfo member)
		{
			System.Type clazz = GetType(member);

			return new KeyValuePair<System.Type, System.Type>(clazz.GetGenericArguments()[0], clazz.GetGenericArguments()[1]);
		}

		/// <summary>
		/// Get the type of the a Field or Property. 
		/// If is a: Generic Collection or a Array, return the type of the elements.
		/// </summary>
		/// <param name="member">MemberInfo, represent a property or field</param>
		/// <returns>type of the member or collection member</returns>
		public static System.Type GetTypeOfMember(MemberInfo member)
		{
			System.Type clazz = GetType(member);

			if (clazz.IsArray) // Is Array
			{
				return clazz.GetElementType();
			}
			else if (IsEnumerable(clazz) && clazz.IsGenericType) //Is Collection Generic  
			{
				return clazz.GetGenericArguments()[0];
			}

			return clazz; //Single type, not a collection/array
		}

		/// <summary>
		/// Indicates if a <see cref="Type"/> is <see cref="IEnumerable"/>
		/// </summary>
		/// <param name="clazz"></param>
		/// <returns>is enumerable or not</returns>
		public static bool IsEnumerable(System.Type clazz)
		{
			return typeof (IEnumerable).IsAssignableFrom(clazz);
		}

		public static bool IsGenericDictionary(System.Type clazz)
		{
			if (clazz.IsInterface && clazz.IsGenericType)
				return typeof(IDictionary<,>).Equals(clazz.GetGenericTypeDefinition());
			else
				return clazz.GetInterface(typeof(IDictionary<,>).Name) == null ? false : true;
		}

		/// <summary>
		/// Get the <see cref="Type"/> of a <see cref="MemberInfo"/>.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static System.Type GetType(MemberInfo member)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Field:
					return ((FieldInfo)member).FieldType;

				case MemberTypes.Property:
					return ((PropertyInfo)member).PropertyType;
				default:
					throw new ArgumentException("The argument must be a property or field", "member");
			}
		}

		/// <summary>
		/// Get the value of some Property or Field.
		/// </summary>
		/// <param name="bean"></param>
		/// <param name="member"></param>
		/// <returns></returns>
		public static object GetMemberValue(object bean, MemberInfo member)
		{
			try
			{
				FieldInfo fi = member as FieldInfo;
				if (fi != null)
					return fi.GetValue(bean);

				PropertyInfo pi = member as PropertyInfo;
				if (pi != null)
					return pi.GetValue(bean, null);
			}
			catch(Exception e)
			{
				throw new InvalidStateException("Could not get property value", e);
			}
			return null;
		}

		public static MemberInfo GetPropertyOrField(System.Type currentClass, string name)
		{
			MemberInfo memberInfo = currentClass.GetProperty(name, AnyVisibilityInstance);
			if (memberInfo == null)
			{
				memberInfo = currentClass.GetField(name, AnyVisibilityInstance);
			}

			return memberInfo;
		}
	}
}