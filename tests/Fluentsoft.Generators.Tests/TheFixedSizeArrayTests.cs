// Copyright 2023 Fluent Software Corporation
// Author: Vadim Zabavnov (mailto:vzabavnov@fluentsoft.net; mailto:zabavnov@gmail.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Fluentsoft.System;
using Xunit.Abstractions;

namespace Fluentsoft.Generators.Tests;

public partial class TheFixedSizeArrayTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TheFixedSizeArrayTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public struct TheElementType 
    {
        public char C;
        public double D;
        public int N;
    }

    [ValueArray<TheElementType>(5)]
    public partial struct TheFixedSizeArray
    {
    }

    [Fact]
    public void TestInitialization()
    {
        var array = new TheFixedSizeArray(stackalloc TheElementType[]
        {
            new TheElementType { C = 'a', D = 1.1, N = 11},
            new TheElementType { C = 'b', D = 2.2, N = 22},
            new TheElementType { C = 'c', D = 3.3, N = 33},
            new TheElementType { C = 'd', D = 4.4, N = 44},
            new TheElementType { C = 'e', D = 5.5, N = 55},
        });

        Assert.Equal('a', array[0].C);
        Assert.Equal(2.2, array[1].D);
        Assert.Equal(33, array[2].N);
    }

    [Fact]
    public unsafe void TestArraySize() 
    {
        var elementSize = sizeof(TheElementType);
        var arraySize = sizeof(TheFixedSizeArray);
        Assert.Equal(elementSize * TheFixedSizeArray.Length, arraySize);
    }

    [Fact]
    public void TestValueType()
    {
        var type = typeof(TheFixedSizeArray);
        Assert.True(type.IsValueType);
    }

    [Fact]
    public void TestFixedSize()
    {
        Assert.Equal(5, TheFixedSizeArray.Length);

        var array = new TheFixedSizeArray();
        Assert.Equal(5, array.Count);
    }

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
        Assert.Equal(1968, array[0].N);

        Assert.Throws<IndexOutOfRangeException>(() => array[5]);
    }

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

    [Fact]
    public void TestArrayIteration()
    {
        var array = new TheFixedSizeArray();
        foreach (ref var item in array)
        {
            _testOutputHelper.WriteLine($"C:{item.C}, D:{item.D}, N:{item.N}");
        }
    }

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
}