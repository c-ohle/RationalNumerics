#if NET7_0

namespace Test
{
  // This is just an outline for a stack-like approach
  // only a minimum of standard operations.
  public interface INumberBuilder
  {
    void Push<Number>(Number value) where Number : INumber<Number>;
    Number Pop<Number>() where Number : INumber<Number>;
    void Pop();
    // binary operations
    void Add();
    void Subtract();
    void Multiply();
    void Divide();
    // unary operations
    void Negate();
    void Sqr();
  }

  // INumberBuilder test implementation, non optimized or efficient implemented
  // but works already for all NET7 numeric types and BigRational as well. 
  public class MyNumberBuilder : INumberBuilder
  {
    // for debug only: not mutch effort, simply show stack as list
    public override string ToString()
    {
      return string.Join("; ", Enumerable.Range(0, (int)cpu.mark()).Reverse().
        Select(i => { cpu.get((uint)i, out BigRational r); return r.ToString(); }));
    }
    void INumberBuilder.Push<T>(T value) => cpu.push(CreateChecked<BigRational, T>(value));
    T INumberBuilder.Pop<T>() => CreateChecked<T, BigRational>(cpu.popr());
    void INumberBuilder.Pop() => cpu.pop();
    void INumberBuilder.Add() => cpu.add();
    void INumberBuilder.Subtract() => cpu.sub();
    void INumberBuilder.Multiply() => cpu.mul();
    void INumberBuilder.Divide() => cpu.div();
    void INumberBuilder.Negate() => cpu.neg();
    void INumberBuilder.Sqr() => cpu.sqr();
    private readonly BigRational.CPU cpu = new();
    private static T CreateChecked<T, V>(V value) where T : INumber<T> where V : INumber<V> => T.CreateChecked(value);
  }
}

#endif