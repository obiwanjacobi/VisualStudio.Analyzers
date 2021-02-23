# Visual Studio Analyzers

This code is available as a [Nuget Package](https://www.nuget.org/packages/Jacobi.VisualStudio.Analyzers/) for you to include in your Visual Studio Project.

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
