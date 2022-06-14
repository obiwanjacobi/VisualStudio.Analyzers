# Visual Studio Analyzers

This code is available as a [Nuget Package](https://www.nuget.org/packages/Jacobi.VisualStudio.Analyzers/) for you to include in your Visual Studio Project.

---

## Analyzers

The following analyzers are available.

### Handle Return Value

A warning will be generated when the return value of a method is not handled.

```C#
class C
{
    static string Method()
        => "Hello World";

    static void Main()
    {
        Method();       // will trigger rule
        _ = Method();   // after applying 'Use discard' fix
    }
}
```

---

## Planned Rules

- No Boolean Parameters (when multiple?)
- Use `const` keyword for constants (msdn example)
- No public virtual/abstract methods (Single Responsibility Principle violation)
- Don't call `bool Equals(object)`, implement `IEquatable<T>`.

---

## Todos

- Add published docs to document rational for each rule and link (url) to them in the reporting.

## Roslyn Source Generator ideas

- Dependency Injection (there is StrongInject that is interface/attr based and SourceInject that is attr based)
- Auto generate Clone method implementations `public virtual MyObject Clone()`
https://github.com/mostmand/Cloneable
- Data Mapper? (like AutoMapper but at compile time)
- Conditional auto parameter checking. Using a compiler constant `#` this generator automatically generates parameter (null) checks for all public and protected methods (in public classes).
- Partial application of function arguments. Generate a new function that has an inline implementation of the function with one or more parameters replaced as constants.
https://github.com/JasonBock/PartiallyApplied
- Templates: Generate source code that has certain place holders (template parameters) replaced with either Types, constants or variables. (probably the same as partial application of functions)
- Attachable Interface. Separate the interface impl from the object. Duplicate the source code of a generic interface implementation (or template) to add to an existing (partial) class. https://github.com/beakona/AutoInterface
- An optimized `Enum.ToString()` that `swicth`es on the enum value and uses `nameof()` to get the name at compile time.
- An in-place (C#) text-template (T4/Razor-like?) that contains instructions to build an expression using Linq-Expressions.
- ComWrappers/ComInterface attribute on interfaces and classes generates a complete implementation for ComWrappers: https://docs.microsoft.com/en-us/dotnet/standard/native-interop/tutorial-comwrappers
- Generate throw ArgumentNullException for all attributed non-nullable reference types.
    Can you insert code into an existing method?
