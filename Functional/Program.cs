using System;
using static Functional.PairDemo;

namespace Functional
{
    class Program
    {
        static void Main(string[] args)
        {
            var pair = Cons(9, "hello");
            var first = pair.First();
            var second = pair.Second();

            Console.WriteLine(first);
            Console.WriteLine(second);
            var factorial = Y<int>(CreateFactorial);

            var x = factorial(5);
            Func<Func<int, int>, Func<int, int>> f1 = CreateFib;
            var fib = Y(f1);
            var y = fib(5);


            var f2 = Y<int>(self => n => n == 0 ? 1  : n * self(n - 1));

            var z = f2(5);
        }
    }
}
