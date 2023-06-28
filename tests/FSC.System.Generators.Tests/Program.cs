using FSC.System;

var st = new MyStruct(stackalloc int[]{1,2,3,4,5});

Console.WriteLine(st.Count);

foreach (var s in st)
{
    Console.WriteLine(s);
}

[FixedArray<int>(5)]
public partial struct MyStruct
{

}