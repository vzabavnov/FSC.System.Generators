# Value Type Arrays

> ### An array is a systematic arrangement of similar objects.
> *From Wikipedia, the free encyclopedia*

Every developer knows arrays: it is simple, and power collection with size and random access by index. 
Most developers now: arrays are contiguous and keep items close to each other. 
Some developers now: an index is used to calculation the position for particular element in an array. 
Dotnet developers also now arrays are reference type and each instance of array is in memory heap.

To work with array is enough have array variable and array initialization with size:

```C#
int array = new int[5];
```

With dotnet core there is a Span and stack allocation:
```C#
Span<int> array = stackalloc int[5];
```

Unfortunately, is not possible to embed such arrays to be part of another construct.
When array of fixed size needs to be part of some other construction, class or struct, only way to do it via using unsafe code:

```C#
public unsafe struct MyStruct
{
    public fixed int Array[5];
}
```

There are some issues with this approach:

1.	Uses unsafe code
2.	No size check during runtime
3.	No iteration through elements
4.	When array’s element type is complex, whole element will be copied.

The requirements the fixed size value type array must satisfy are:

1.	All items occupy contiguous region of memory
2.	Is value type and may be allocate on a machine stack
3.	Has fixed size, defined during compilation. The size can be check during runtime. 
Runtime knows boundaries of array and raise [IndexOutOfRangeException](https://learn.microsoft.com/en-us/dotnet/api/system.indexoutofrangeexception?view=net-7.0) when index out of array’s boundaries
4.	Have access to element by index
5.	Elements of arrays accessible by reference
6.	Supports iteration through all elements
7.	Can be a part of any classes or structure without using unsafe code

## Value Type Array

Let see what construct possible to satisfy criteria above for array of 5 elements of type:

```C#
public struct TheElementType 
{
    public char C;
    public double D;
    public int N;
}
```

```C#
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TheFixedSizeArray 
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]	
    private struct TheStorageStruct {
	    private TheElementType __n0, __n1, __n2, __n3,__n4;
    }
    private TheStorageStruct _storage;

    [UnscopedRef]
    private Span< TheElementType> Span => MemoryMarshal.Cast TheStorageStruct, TheElementType new Span TheStorageStruct >(ref _storage));");
    
    public const int Length = 5;

    public int Count => 5;

    [UnscopedRef]
    public ref TheElementType this[Index idx] => ref Span[idx];

    public Span<TheElementType>.Enumerator GetEnumerator() => Span.GetEnumerator();;
}
```

The **TheFixedSizeArray** satisfies all criteria above:
1. All items allocated in the TheStorageStruct and take continue region of memory with size correct size:
    ```C#
    [Fact]
    public unsafe void TestArraySize() 
    {
	    var elementSize = sizeof(TheElementType);
	    var arraySize = sizeof(TheFixedSizeArray);
	    Assert.Equal(elementSize * TheFixedSizeArray.Length, arraySize);
    }
    ```
2. The array is **struct** and **struct** is value type:
    ```C#
    [Fact]
    public void TestValueType()
    {
        var type = typeof(TheFixedSizeArray);
        Assert.True(type.IsValueType);
    }
    ```
3. There is `const Length` and runtime `Count`:
    ```C#
    [Fact]
    public void TestFixedSize()
    {
        Assert.Equal(5, TheFixedSizeArray.Length);

        var array = new TheFixedSizeArray();
        Assert.Equal(5, array.Count);
    }
    ```
4. There is an indexer:
    ```C#
    [Fact]
    public void TestAccessByIndex()
    {
        var array = new TheFixedSizeArray();
        var element = new TheElementType
        {
            C = 'C',
            D = 3.14,
            N = 1968
        };
        array[0] = element;
        Assert.Equal('C', array[0].C);
        Assert.Equal(3.14, array[0].D);
        Assert.Equal(31968, array[0].N);
    }
    ```
5. Access by reference - available:
    ```C#
    [Fact]
    public void TestAccessByRef()
    {
        var array = new TheFixedSizeArray
        {
            [0] = new()
            {
                C = 'C',
                D = 3.14,
                N = 1968
            }
        };

        ref var item = ref array[0];
        item.D = 1.61;

        Assert.Equal(1.61, array[0].D);
    }
    ```
6. The iteration through items:
    ```C#
    [Fact]
    public void TestArrayIteration()
    {
        var array = new TheFixedSizeArray();
        foreach (ref var item in array)
        {
            _testOutputHelper.WriteLine($"C:{item.C}, D:{item.D}, N:{item.N}");
        }
    }
    ```
7. Can be placed everywhere:
    ```C#
    public struct Struct1
    {
        public int Value;
    }

    public struct Struct2
    {
        public TheFixedSizeArray Array;
    }

    public struct Struct3
    {
        public Struct1 Struct1;
        public Struct2 Struct2;
    }

    [Fact]
    public void TestArrayPlacement()
    {
        var st = new Struct3();
        ref var item = ref st.Struct2.Array[0];
        item.N = 38;

        Assert.Equal(38, st.Struct2.Array[0].N);
    }
    ```

## Value Type Array Generator
Only problem with construction above – it tedious to write such construction by hand, especially when number if items is more than few.

Donet core supports code generators and it is very useful for generation of value type arrays.

The nuget package [Fluentsoft.Generators](https://www.nuget.org/packages/Fluentsoft.Generators) contains code attribute `ValueArrayAttribute`. 
The `ValueArrayAttribute` applicable to any `struct`, and will embed array's functionality to the struct the attribute applied onto:
```C#
[ValueArray<TheElementType>(5)]
public partial struct TheFixedSizeArray
{}
```

The `ValueArrayAttribute` takes type of element as generic argument and a number of elements in the array. 
Resulting construction will have following members:

|Member|Description|
|------|-----------|
|const int Length|Length of the array|
|Type ElementType|The type of array's element|
|int Count|Number of item in the array|
|Span\<ElementTypeName> Span|The [Span\<T\>](https://learn.microsoft.com/en-us/dotnet/api/system.span-1?view=net-7.0) for embedded storage
|ref ElementTypeName this[Index idx]|The indexer|
|ToArray()|Copy all items to resulting array|
|CopyTo(ElementTypeName[] target, int index)|Copies all the elements of the current one-dimensional array to the specified one-dimensional array starting at the specified destination array index|
|CopyTo(Span\<ElementTypeName\> target)|Copies the contents of this array into a destination Span\<T>\|
|CopyTo(Memory\<ElementTypeName\> target)|Copies the contents of this array into a destination Memory\<T>\|
|impilict operator ReadonlySpan\<ElementTypeName>|Uses an array as a [ReadonlySpan\<T>](https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1?view=net-7.0)|


This article and [codegenerator](https://www.nuget.org/packages/Fluentsoft.Generators) to support value type arrays written by [Vadim Zabavnov](https://github.com/vzabavnov/), at June of 2023.  
The nuget package: [Fluentsoft.Generators](https://www.nuget.org/packages/Fluentsoft.Generators)  
The github repository: [Fluentsoft.Generators](https://github.com/vzabavnov/Fluentsoft.Generators)

If you like it and find it usable, please 
<form action="https://www.paypal.com/donate" method="post" target="_top">
<input type="hidden" name="business" value="ZXE6QAL7SBP4J" />
<input type="hidden" name="no_recurring" value="0" />
<input type="hidden" name="currency_code" value="USD" />
<input type="image" src="https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif" border="0" name="submit" title="PayPal - The safer, easier way to pay online!" alt="Donate with PayPal button" />
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1" />
</form>
