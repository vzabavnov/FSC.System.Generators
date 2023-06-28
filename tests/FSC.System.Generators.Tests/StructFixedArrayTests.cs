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

using FSC.System.Generators.Tests.AnotherOne;

namespace FSC.System.Generators.Tests;

public class StructFixedArrayTests
{
    [Fact]
    public unsafe void TestFixedStruct()
    {
        var stSize = sizeof(My);

        var f = new FixedStructArray();
        Assert.Equal(3, FixedStructArray.Length);

        Assert.Equal(stSize * FixedStructArray.Length, sizeof(FixedStructArray));

        f[0].C = 'A';
        Assert.Equal('A', f[0].C);

        f[1].N = 15;
        Assert.Equal(15, f[1].N);

        Assert.Throws<IndexOutOfRangeException>(() => f[3]);

        var arr = f.ToArray();
        Assert.Equal(3, arr.Length);
        Assert.Equal('A', arr[0].C);
        Assert.Equal(15, arr[1].N);

        var st = new MyStruct(stackalloc int[]{1,2,3,4,5});
        Assert.Equal(5, st.Count);
        Assert.Equal(new int[]{1,2,3,4,5}, st.ToArray());
    }
}