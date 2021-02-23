using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Jacobi.VisualStudio.Analyzers.Test.CSharpCodeFixVerifier<
    Jacobi.VisualStudio.Analyzers.HandleReturnValueAnalyzer,
    Jacobi.VisualStudio.Analyzers.HandleReturnValueCodeFix>;

namespace Jacobi.VisualStudio.Analyzers.Test
{
    [TestClass]
    public class HandleReturnValueTests
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task Empty()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task NotApplicable()
        {
            var test = @"
class MyClass
{
    static void Run()
    {
        var v = 42;
        throw new System.NotImplementedException();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task Unhandled()
        {
            var test = @"
class MyClass
{
    static int FourtyTwo()
        => 42;

    static void Run()
    {
        {|#0:FourtyTwo()|};
    }
}
";

            var expected = VerifyCS.Diagnostic(HandleReturnValueAnalyzer.DiagnosticId)
                .WithLocation(0).WithArguments("FourtyTwo()");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task FixedUnhandled()
        {
            var test = @"
class MyClass
{
    static int FourtyTwo()
        => 42;

    static void Run()
    {
        {|#0:FourtyTwo()|};
    }
}
";

            var fixup = @"
class MyClass
{
    static int FourtyTwo()
        => 42;

    static void Run()
    {
        _ = FourtyTwo();
    }
}
";
            var expected = VerifyCS.Diagnostic(HandleReturnValueAnalyzer.DiagnosticId)
                .WithLocation(0).WithArguments("FourtyTwo()");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixup)
                ;
        }

        [TestMethod]
        public async Task UnhandledAsync()
        {
            var test = @"
using System.Threading.Tasks;
class MyClass
{
    static Task<int> FourtyTwo()
        => Task.FromResult(42);

    static async void Run()
    {
        await {|#0:FourtyTwo()|};
    }
}
";

            var expected = VerifyCS.Diagnostic(HandleReturnValueAnalyzer.DiagnosticId)
                .WithLocation(0).WithArguments("FourtyTwo()");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task VariableInitialization()
        {
            var test = @"
class MyClass
{
    static int FourtyTwo()
        => 42;

    static void Run()
    {
        var x = FourtyTwo();
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }


        [TestMethod]
        public async Task Assignment()
        {
            var test = @"
class MyClass
{
    static void Take42(int value)
    {
        if (value != 42) throw new System.ArgumentException();
    }

    static int FourtyTwo()
        => 42;

    static void Run()
    {
        int v;
        v = FourtyTwo();
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task CallParameter()
        {
            var test = @"
class MyClass
{
    static void Take42(int value)
    {
        if (value != 42) throw new System.ArgumentException();
    }

    static int FourtyTwo()
        => 42;

    static void Run()
    {
        Take42(FourtyTwo());
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task ReturnDirect()
        {
            var test = @"
using System.Threading.Tasks;
class MyClass
{
    static int FourtyTwo()
        => 42;

    static int Run()
        => FourtyTwo();
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task AwaitTask()
        {
            var test = @"
using System.Threading.Tasks;
class MyClass
{
    static Task<int> FourtyTwo()
        => Task.FromResult(42);

    static async void Run()
    {
        var x = await FourtyTwo();
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task AwaitVoidTask()
        {
            var test = @"
using System.Threading.Tasks;
class MyClass
{
    static Task VoidAsync()
        => Task.CompletedTask;

    static async void Run()
    {
        await VoidAsync();
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task ExpressionIf()
        {
            var test = @"
class MyClass
{
    static int FourtyTwo()
        => 42;

    static void Run()
    {
        if (FourtyTwo() == 42)
            throw new System.Exception();
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task ExpressionArithmetic()
        {
            var test = @"
class MyClass
{
    static int FourtyTwo()
        => 42;

    static void Run()
    {
        if (FourtyTwo() + 2 == 42)
            throw new System.Exception();
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task Tuple()
        {
            var test = @"
class MyClass
{
    static int FourtyTwo()
        => 42;

    static void Run()
    {
        var t = (42, FourtyTwo());
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}
