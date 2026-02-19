using System.Reflection;
using TimeToKill.Extensions;
#pragma warning disable CS0168 // Variable is declared but never used

namespace TimeToKill.Tools;

public static class ReflectTools
{
	// Iterates (PropName, PropValue) tuples for all properties.
	// Example: IteratePropertiesWithValues<MyClass, string>(myInstance, true)
	public static IEnumerable<(string Name, PT Value)> IteratePropertiesWithValues<BT, PT>(BT instance, bool nonNullOnly = false)
	{
		if (instance is null) {
			yield break;
		}
		
		foreach (var property in GetInstanceProperties<BT, PT>()) {
			var value = property.GetValue(instance);
			if (nonNullOnly && value == null) {
				continue;
			}
			
			if (value is PT typedValue) {
				yield return (property.Name, typedValue);
			}
		}
	}
	
	// Enumerates all instance properties of type PT (ParamType) on type BT (BaseType).
	// Example: GetInstanceProperties<MyClass, string>() returns all string properties on MyClass.
	public static IEnumerable<PropertyInfo> GetInstanceProperties<BT, PT>()
	{
		var properties = typeof(BT).GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var propType = typeof(PT);
		
		properties = properties
			.Where(p => p.PropertyType.IsAssignableFrom(propType))
			.ToArray();
		
		return properties;
	}
	
	// Helper function to smush string properties from one object into another.
	// Only overwrites target properties if they are null or empty.
	// Example: MergeStringProperties<MyClass>(sourceInstance, targetInstance)
	public static bool MergeStringProperties<T>(T fromSource, T intoTarget)
	{
		try {
			foreach (var property in GetInstanceProperties<T, string>()) {
				var sourceValue = property.GetValue(fromSource) as string;
				var targetValue = property.GetValue(intoTarget) as string;
				
				// If null, simply overwrite.
				if (targetValue is null && sourceValue.HasValue()) {
					property.SetValue(intoTarget, sourceValue);
				} else if (targetValue != null && !targetValue.HasValue() && sourceValue.HasValue()) {
					property.SetValue(intoTarget, sourceValue);
				}
			}
		} catch (Exception e) {
			return false;
		}
		
		return true;
	}
}
