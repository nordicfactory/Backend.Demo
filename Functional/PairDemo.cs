using System;

namespace Functional
{
    public static class PairDemo
    {
        public static Pair<T1, T2> Cons<T1, T2>(T1 t1, T2 t2) => f => f(t1, t2);
        public static T1 First<T1, T2>(this Pair<T1, T2> p) => (T1) p.Invoke((t, _) => t);
        public static T2 Second<T1, T2>(this Pair<T1, T2> p) => (T2) p.Invoke((_, t) => t);
        public delegate object Pair<T1, T2>(Func<T1, T2, object> f);


        public static Func<T,T> Y<T>(Func<Func<T,T>, Func<T,T>> f)
        {
            //dynamic here is in fact a func from T to T.
            //so x takes a func from T to T, and return a "new" func from T to T, that is recursive.
            //But if it is from T to T, how can we pass it to itself? Since it is recursive!
            // in the base-case we don't call this function, so it does not matter that it cannot produce values by itself.
            //
            static Func<T,T> Y1(Func<dynamic, Func<T,T>> x) => x(x);

            return Y1(x =>
            {
                return f(y =>
                {
                    Func<T,T> z = x(x);
                    return z(y);
                });
            });
        }

        public static Func<int, int> CreateFactorial(Func<int, int> self)
        {
            int FactorialRec(int n)
            {
                return n == 0 
                    ? 1 
                    : n * self(n - 1);
            }

            return FactorialRec;
        }

        public static Func<int, int> CreateFib(Func<int,int> self)
        {
            int Fib(int n)
            {
                return n switch
                {
                    0 => 1,
                    1 => 1,
                    _ => (self(n - 2) + self(n - 1))
                };
            }

            return Fib;
        }
    }

}
