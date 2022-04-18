# NewRational - a novel rational number class.
Documentation available on [github-pages](https://c-ohle.github.io/RationalNumerics/).

<!--
| Syntax       | Description |
| ------------ | ----------- |
| Header       | Title       |
| Paragraph    | Text        |
| Paragraph2   | Text2       |

| Syntax       | Description | Size          |
| :---         |    :----:   |          ---: |
| Header       | Title       | 123           |
| Paragraph    | Text        |               |  
| Paragraph2   | Text2       |			55	 |	

```c#
public static Vector3R Cross(Vector3R a, Vector3R b)
{
  var cpu = NewRational.task_cpu;
  cpu.mul(a.X, b.Y); cpu.mul(a.Y, b.X); cpu.sub(); // z
  cpu.mul(a.Z, b.X); cpu.mul(a.X, b.Z); cpu.sub();
  cpu.mul(a.Y, b.Z); cpu.mul(a.Z, b.Y); cpu.sub();
  return new Vector3R(cpu.pop_rat(), cpu.pop_rat(), cpu.pop_rat());
}
```
-->