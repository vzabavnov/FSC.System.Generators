# Fluentsoft Generators

Package: [Fluentsoft.Generators](https://www.nuget.org/packages/Fluentsoft.Generators)  
Assembly: fluentsoft.generators.dll, fluentsoft.generators.runtime.dll  

## Value Arrays 
Generates value array of fixed size.
### ValueArrayAttribute\<T\> class
`where T : unmanaged`

Namespace: Fluentsoft.System  
Assembly: fluentsoft.generators.runtime.dll  

Marks a _struct_ as a placeholder for value array:
```c#
[ValueArray<int>(5)]
public partial struct ValueArrayStruct { }
```
### Remarks
When struct marked by ValueArray attribute, the code generator add folowing members to the _struct_ type  
The **StructTypeName** is a name of struct the ValueArrayAttribute applied to.  
The **ElementTypeName** is a name of array element type.

**Constructors**  

| |
|-|
|StructTypeName(Span\<ElementTypeName\> source)|
|StructTypeName(Memory\<ElementTypeName\> source)|
|StructTypeName(ElementTypeName[] source)|

**Members**

|Member|Description|
| - | - |
|const int Length|Length of the array|
|Type ElementType|The type of array's element|
|int Count|Number of item in the array|
|Span\<ElementTypeName> Span|
|ref ElementTypeName this[Index idx]||

**Methods**

|Method|Description|
| - | - |
|ToArray()|Copy all items to resulting array|
|CopyTo(ElementTypeName[] target, int index)|Copies all the elements of the current one-dimensional array to the specified one-dimensional array starting at the specified destination array index.|
|CopyTo(Span\<ElementTypeName\> target)|Copies the contents of this array into a destination Span\<T>\.|
|CopyTo(Memory\<ElementTypeName\> target)||

### Example
```c#
using Fluentsoft.System;
using Xunit;

Assert.Equal(3, ValueArrayStruct.Length);
Assert.Equal(3 * sizeof(My), sizeof(ValueArrayStruct))

var f = new ValueArrayStruct();
f[0].C = 'A';
f[1].N = 15;
var m1 = f[0];
ref var m2 = ref f[1];
Assert.Equal(f[1].N, m2.N);
m2.N = 38;
Assert.Equa;(38, f[1].N);

Assert.Throws<IndexOutOfRangeException>(() => f[3]);

var st = new MyStruct(stackalloc int[]{1,2,3,4,5});
foreach (var s in st)
{
    Console.WriteLine(s);
}
Assert.Equal(new int[]{1,2,3,4,5}, st.ToArray());

public struct My
{
    public char C;
    public double D;
    public int N;
}

[ValueArray<My>(3)]
public partial struct ValueArrayStruct { }

[ValueArray<int>(5)]
public partial struct MyStruct { }
```
