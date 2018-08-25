namespace NUnit.Engine
{
    internal delegate TResult Func<out TResult>();
    internal delegate TResult Func<in T, out TResult>(T arg);
}
