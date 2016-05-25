using System;
using NUnit.Framework;
using System.Collections.Generic;
namespace ProtoFFITests
{
    public class TestData
    {
        public static double MultiplyDoubles(double x, double y)
        {
            return x * y;
        }
        public static double MultiplyFloats(float x, float y)
        {
            return x * y;
        }
        public static float GetFloat()
        {
            return 2.5F;
        }
        public static decimal MultiplyDecimals(decimal x, decimal y)
        {
            return Decimal.Multiply(x, y);
        }
        public static byte IncrementByte(byte value)
        {
            return ++value;
        }
        public static sbyte IncrementSByte(sbyte value)
        {
            return ++value;
        }
        public static char GetAlphabet(int index)
        {
            int c = 'a';
            return (char)(c + index);
        }
        public static char ToUpper(char c)
        {
            return char.ToUpper(c);
        }
        public static char ToChar(object o)
        {
            return (char)(int)o;
        }
        public static int ToAscii(char c)
        {
            return c;
        }
        public static int Combine(byte x, byte y)
        {
            return x << 8 | y;
        }
        public static long MultiplyShorts(short x, short y)
        {
            return x * y;
        }
        public static long MultiplyUShorts(ushort x, ushort y)
        {
            return x * y;
        }
        public static long MultiplyUInts(uint x, uint y)
        {
            return x * y;
        }
        public static ulong MultiplyULongs(ulong x, ulong y)
        {
            return x * y;
        }
        public static bool Equals(float x, float y)
        {
            return Math.Abs(x - y) < 0.0001;
        }
        public static bool Equals(Decimal x, Decimal y)
        {
            return Decimal.Equals(x, y);
        }
        public static IEnumerable<int> GetSomePrimes()
        {
            return new List<int> { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 };
        }
        public static IEnumerable<int> GetNumbersByDouble(int x)
        {
            for (int i = 0; i < x; ++i)
            {
                yield return i * 2;
            }
        }
        public static IEnumerable<int> DoubleThem(IEnumerable<int> nums)
        {
            foreach (var item in nums)
            {
                yield return item * 2;
            }
        }
        public object[] GetMixedObjects()
        {
            object[] objs = { new DerivedDummy(), new Derived1(), new TestDispose(), new DummyDispose() };
            return objs;
        }
        public override bool Equals(Object obj)
        {
            return true;
        }
        public object FuncReturningVariousObjectTypes(int x)
        {
            switch (x)
            {
                case 0:
                    {
                        ulong u = 1;
                        return u;
                    }
                case 1:
                    {
                        Byte b = 1;
                        return b;
                    }
                case 2:
                    {
                        sbyte s = 1;
                        return s;
                    }
                case 3:
                    {
                        short s = 1;
                        return s;
                    }
                case 4:
                    {
                        UInt16 u = 1;
                        return u;
                    }
                case 5:
                    {
                        return new DummyDispose();
                    }
                case 6:
                    {
                        UInt64 u = 1;
                        return u;
                    }
                case 7:
                    {
                        char c = '1';
                        return c;
                    }
                case 8:
                    {
                        float f = 1;
                        return f;
                    }
                case 9:
                    {
                        Decimal d = 1;
                        return d;
                    }
                case 10:
                    {
                        ushort u = 1;
                        return u;
                    }
                case 11:
                    {
                        return new DerivedDummy();
                    }
                case 12:
                    {
                        return new TestDisposeDerived();
                    }
                case 13:
                    {
                        return new Derived1();
                    }
                case 14:
                    {
                        return new TestDispose();
                    }
                case 15:
                    {
                        string s = "test";
                        return s;
                    }
                case 16:
                    {
                        int i = 1;
                        return i;
                    }
                case 17:
                    {
                        double d = 1;
                        return d;
                    }
                case 18:
                    {
                        Boolean b = true;
                        return b;
                    }
                default:
                    return 0;
            }
        }
        public int TestUlong(ulong x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestUlong2(object x)
        {
            ulong y = Convert.ToUInt64(x);
            if (y == 1)
                return 1;
            else return 0;
        }
        public int TestByte(Byte x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestSbyte(sbyte x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestShort(short x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestUint16(UInt16 x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestDummyDispose(DummyDispose x)
        {
            return x.Value;
        }
        public int TestUint64(UInt64 x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestChar(Char x)
        {
            if (x == '1')
                return 1;
            else return 0;
        }
        public int TestFloat(float x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestDecimal(Decimal x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestUshort(ushort x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public double TestDerivedDummy(DerivedDummy x)
        {
            return x.random123();
        }
        public int TestDerivedDummyClass(DerivedDummy x)
        {
            return x.random123();
        }
        public int TestDerivedDisposeClass(TestDisposeDerived x)
        {
            return x.get_MyValue();
        }
        public double TestDerived1(Derived1 x)
        {
            return x.GetNumber();
        }
        public int TestDisposeClass(TestDispose x)
        {
            return x.get_MyValue();
        }
        public int TestString(String x)
        {
            return x.Length;
        }
        public int TestInt(int x)
        {
            return x;
        }
        public int TestInt2(object x)
        {
            int y = Convert.ToInt32(x);
            return y;
        }
        public int TestDouble(Double x)
        {
            if (x == 1)
                return 1;
            else return 0;
        }
        public int TestBoolean(Boolean x)
        {
            if (x == true)
                return 1;
            else return 0;
        }
        public int TestIEnumerable(IEnumerable<int> x)
        {
            IEnumerator<int> y2 = x.GetEnumerator();
            y2.Reset();
            y2.MoveNext();
            return y2.Current;
        }
        public int TestIEnumerable2(object x)
        {
            IEnumerable<int> y = (IEnumerable<int>)x;
            IEnumerator<int> y2 = y.GetEnumerator();
            y2.Reset();
            y2.MoveNext();
            return y2.Current;
        }
        public object GetIEnumerable()
        {
            return new List<int> { 2, 2, 2, 2 };
        }
        public object GetInt()
        {
            int x = 1;
            return x;
        }
        public object GetUlong()
        {
            ulong x = 1;
            return x;
        }
        public Object FuncReturningByteAsObject()
        {
            Byte b = 1;
            return b;
        }
        public double FuncVerifyingVariousObjectTypes(object y, int x)
        {
            switch (x)
            {
                case 0: return this.TestUlong(Convert.ToUInt64(y));
                case 1: return this.TestByte(Convert.ToByte(y));
                case 2: return this.TestSbyte(Convert.ToSByte(y));
                case 3: return this.TestShort(Convert.ToInt16(y));
                case 4: return this.TestUint16(Convert.ToUInt16(y));
                case 5: return this.TestDummyDispose((DummyDispose)y);
                case 6: return this.TestUint64(Convert.ToUInt64(y));
                case 7: return this.TestChar(Convert.ToChar(y));
                case 8: return this.TestFloat(Convert.ToSingle(y));
                case 9: return this.TestDecimal(Convert.ToDecimal(y));
                case 10: return this.TestUshort(Convert.ToUInt16(y));
                case 11: return this.TestDerivedDummyClass((DerivedDummy)y);
                case 12: return this.TestDerivedDisposeClass((TestDisposeDerived)y);
                case 13: return this.TestDerived1((Derived1)y);
                case 14: return this.TestDisposeClass((TestDispose)y);
                case 15: return this.TestString(Convert.ToString(y));
                case 16: return this.TestInt(Convert.ToInt32(y));
                case 17: return this.TestDouble(Convert.ToDouble(y));
                case 18: return this.TestBoolean(Convert.ToBoolean(y));
                default:
                    return -1;
            }
        }
        public object CreateInternalClass(int y)
        {
            return InternalClass.CreateObject(5);
        }
        public int TestInternalClass(object y)
        {
            InternalClass x = (InternalClass)y;
            return x.GetValue();
        }
    }
    internal class InternalClass
    {
        private int x = 5;
        public static InternalClass CreateObject(int y)
        {
            return new InternalClass { x = y };
        }
        public int GetValue()
        {
            return x;
        }
    }
    public class MethodOverloadingClass
    {
        float f = 1.5F;
        public float GetValue()
        {
            return f;
        }
        public int foo(double x)
        {
            return 1;
        }
        public int foo(float x)
        {
            return 0;
        }
    }
    class CSFFIDataMarshalingTest : FFITestSetup
    {

        [Test]
        public void TestDoubles()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 123454321.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestFloats()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestFloatOut()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestFloatsOutOfRangeWarning()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = null;
            Assert.IsTrue(ExecuteAndVerify(code, data) == 1);
        }

        [Test]
        public void TestDecimals()
        {
            String code =
            @"
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            int nErrors = -1;
            ExecuteAndVerify(code, data, out nErrors);
            Assert.IsTrue(nErrors == 0);
        }

        [Test]
        public void TestChar()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            int F = 'F';
            ValidationData[] data = { new ValidationData { ValueName = "F", ExpectedValue = F, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestCharOutOfRangeWarning()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = null;
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestByte()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            int F = 'F';
            ValidationData[] data = { new ValidationData { ValueName = "F", ExpectedValue = F, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestByteOutOfRangeWarning()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = null;
            Assert.IsTrue(ExecuteAndVerify(code, data) == 1);
        }

        [Test]
        public void TestSByte()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            int F = 'F';
            ValidationData[] data = { new ValidationData { ValueName = "F", ExpectedValue = F, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestSByteOutOfRangeWarning()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = null;
            Assert.IsTrue(ExecuteAndVerify(code, data) == 1);
        }

        [Test]
        public void TestCombineByte()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 25700, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestShort()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 10000, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestUShort()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 10000, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestUInt()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 10000, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestULong()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 10000, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestNullForPrimitiveType() //Defect 1462014 
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "bytevalue", ExpectedValue = null, BlockIndex = 0 },
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestIEnumerable()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "prime", ExpectedValue = 13, BlockIndex = 0 },
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestIEnumerable2()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 10, BlockIndex = 0 },
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestIEnumerable3()
        {
            String code =
            @"
            Type t = Type.GetType("ProtoFFITests.TestData"); //"ProtoFFITests.TestData, ProtoTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            code = string.Format("import(\"{0}\");\r\n{1}", t.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 10, BlockIndex = 0 },
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_IEnumerable_Implicit_Cast()
        {
            string code =
                @" t = TestData.TestData();
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 2, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_IEnumerable_Explicit_Cast()
        {
            string code =
                @" t = TestData.TestData();
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 2, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Int_Implicit_Cast()
        {
            string code =
                @" t = TestData.TestData();
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 1, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Int_Explicit_Cast()
        {
            string code =
                @" t = TestData.TestData();
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 1, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Ulong_Implicit_Cast()
        {
            string code =
                @" t = TestData.TestData();
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 1, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Ulong_Explicit_Cast()
        {
            string code =
                @" t = TestData.TestData();
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 1, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Over_Internal_Classes()
        {
            string code =
                @" t = TestData.TestData();
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 5, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Using_Implicit_Type_Cast_In_Method_Arguments()
        {
            string code =
                @" t = TestData.TestData();
            object[] b = new object[] { 1, 1, 1, 1, 1, 20, 1, 1, 1, 1, 1, 123, 5, 20.0, 5, 4, 1, 1, 1 };
            ValidationData[] data = { new ValidationData { ValueName = "t21", ExpectedValue = b, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_DataMasrshalling_Using_Explicit_Type_Cast_In_Methods()
        {
            string code =
                @" t = TestData.TestData();
            object[] b = new object[] { 1.0, 1.0, 1.0, 1.0, 1.0, 20.0, 1.0, 1.0, 1.0, 1.0, 1.0, 123.0, 5.0, 20.0, 5.0, 4.0, 1.0, 1.0, 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = b, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.TestData");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Test_MethodOverloading_In_Csharp_Classes()
        {
            string code =
                @" t = MethodOverloadingClass.MethodOverloadingClass();
            ValidationData[] data = { new ValidationData { ValueName = "t2", ExpectedValue = 0, BlockIndex = 0 } };
            Type dummy = Type.GetType("ProtoFFITests.MethodOverloadingClass");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ExecuteAndVerify(code, data);
        }
    }
}