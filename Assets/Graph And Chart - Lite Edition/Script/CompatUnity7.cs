


#if !UNITY_6000_5_OR_NEWER
namespace ChartAndGraph
{
	using System;

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
	internal class AutoStaticsCleanupAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false)]
	internal class NoAutoStaticsCleanupAttribute : Attribute { }
}

namespace Unity.Scripting.LifecycleManagement
{
}
#endif
