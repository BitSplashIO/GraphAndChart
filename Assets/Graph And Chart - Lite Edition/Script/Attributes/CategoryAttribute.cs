using System;

/// <summary>
/// Groups a serialized field under a named category in the inspector.
/// SortingOrder sorts the field within its category (lower = higher up).
/// Place this file OUTSIDE of any "Editor" folder (runtime assembly).
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class CategoryAttribute : Attribute
{
    public readonly string Name;
    public readonly int SortingOrder;

    public CategoryAttribute(string name, int sortingOrder = 0)
    {
        Name = name;
        SortingOrder = sortingOrder;
    }
}

/// <summary>
/// Assigned to a class. Determines the position of a category in the inspector
/// (lower = higher up). Use one attribute per category.
/// Categories without a CategoryOrder appear after the ordered ones,
/// in the order they are declared in the class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class CategoryOrderAttribute : Attribute
{
    public readonly string Name;
    public readonly int Order;

    public CategoryOrderAttribute(string name, int order)
    {
        Name = name;
        Order = order;
    }
}
