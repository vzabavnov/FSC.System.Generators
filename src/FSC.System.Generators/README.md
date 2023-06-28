# FSC Generators
## Definitions 
Package: [FSC.Generators](https://www.nuget.org/packages/FSC.Generators)  
Assembly: fsc.system.generators.dll, fsc.system.generators.runtime.dll  


## Fixed Size Array Generator
Generates fixed size array.
### FixedArrayAttribute\<T\> class
Namespace: FSC.System  
Assembly: fsc.system.generators.runtime.dll  

Specify generating fixed size array on the `struct` type.
```c#
[FixedArray<int>(5)]
public partial struct FixedStructArray { }
```
### Remarks
When struct marked by FixedArray attribute, the code generator add folowing members to the struct type  

**Constructors**

| | |
|-|-|
|\{typeName\}(Span\<\{itemTypeName\}\> source)||
|\{typeName\}(Memory\<\{itemTypeName\}\> source)||
|\{typeName\}(\{itemTypeName\}[] source)||

**Property, Fields**

|Member|Description|
| - | - |
|Length|Length of the array|
|ElementType|The type of array's element|
|Count|Number of item in the array|
|Span\<\{itemTypeName\}>|
|this[Index idx]||

**Methods**

|Member|Description|
| - | - |
|ToArray()||
|CopyTo(\{itemTypeName\}[] target, int index)||
|CopyTo(Span\<\{itemTypeName\}\> target)||
|CopyTo(Memory\<\{itemTypeName\}\> target)||


### Example
```c#
using FSC.System;

Assert.Equal(3, FixedStructArray.Length);
Assert.Equal(3 * sizeof(My), sizeof(FixedStructArray))

var f = new FixedStructArray();
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

[FixedArray<My>(3)]
public partial struct FixedStructArray { }

[FixedArray<int>(5)]
public partial struct MyStruct { }
```

term
: definition  
: qqq
