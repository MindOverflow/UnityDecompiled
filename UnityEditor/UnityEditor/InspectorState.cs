using System;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace UnityEditor
{
	internal sealed class InspectorState
	{
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void SetBool(string key, bool value);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool GetBool(string key, bool defaultValue);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void EraseBool(string key);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void SetFloat(string key, float value);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern float GetFloat(string key, float defaultValue);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void EraseFloat(string key);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void SetInt(string key, int value);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int GetInt(string key, int defaultValue);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void EraseInt(string key);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void SetString(string key, string value);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string GetString(string key, string defaultValue);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void EraseString(string key);
		internal static void SetVector3(string key, Vector3 value)
		{
			InspectorState.INTERNAL_CALL_SetVector3(key, ref value);
		}
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_SetVector3(string key, ref Vector3 value);
		internal static Vector3 GetVector3(string key, Vector3 defaultValue)
		{
			return InspectorState.INTERNAL_CALL_GetVector3(key, ref defaultValue);
		}
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Vector3 INTERNAL_CALL_GetVector3(string key, ref Vector3 defaultValue);
		[WrapperlessIcall]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void EraseVector3(string key);
	}
}