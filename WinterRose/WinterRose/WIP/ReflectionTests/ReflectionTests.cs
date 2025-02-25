using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using opcodes = System.Reflection.Emit.OpCodes;

namespace WinterRose.WIP.ReflectionTests
{
    /// <summary>
    /// Random tests for reflection.
    /// </summary>
    public class ReflectionTests
    {
        public Type GetCustomAssembly()
        {
            AssemblyName aName = new AssemblyName("MyTestAssembly");
            AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndCollect);
 
            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name);

            TypeBuilder tb = mb.DefineType("MyTestType", TypeAttributes.Public);

            FieldBuilder fbNumber = tb.DefineField("_number", typeof(int), FieldAttributes.Private);
            
            Type[] parameterTypes = { typeof(int) };
            ConstructorBuilder ctor1 = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes);
            
            ILGenerator ctor1IL = ctor1.GetILGenerator();
            ctor1IL.Emit(opcodes.Ldarg_0);
            ctor1IL.Emit(opcodes.Ldarg_1);
            ctor1IL.Emit(opcodes.Stfld, fbNumber);
            ctor1IL.Emit(opcodes.Ret);

            ConstructorBuilder c0 = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            ILGenerator c0g = c0.GetILGenerator();
            c0g.Emit(opcodes.Ldarg_0);
            c0g.Emit(opcodes.Ldc_I4_S);
            c0g.Emit(opcodes.Call, ctor1);
            c0g.Emit(opcodes.Ret);

           
            PropertyBuilder pbn = tb.DefineProperty("Number", PropertyAttributes.HasDefault, typeof(int), Type.EmptyTypes);


            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            MethodBuilder mbset = tb.DefineMethod("set_number", getSetAttr, typeof(int), Type.EmptyTypes);
            ILGenerator mbsetgen = mbset.GetILGenerator();
            mbsetgen.Emit(opcodes.Ldarg_0);
            mbsetgen.Emit(opcodes.Ldarg_1);
            mbsetgen.Emit(opcodes.Stfld, fbNumber);
            mbsetgen.Emit(opcodes.Ret);

            MethodBuilder mbget = tb.DefineMethod("get_number", getSetAttr, typeof(int), Type.EmptyTypes);
            ILGenerator mbgetgen = mbget.GetILGenerator();
            mbgetgen.Emit(opcodes.Ldarg_0);
            mbgetgen.Emit(opcodes.Ldfld, fbNumber);
            mbgetgen.Emit(opcodes.Ret);

            pbn.SetSetMethod(mbset);
            pbn.SetGetMethod(mbget);

            MethodBuilder meth = tb.DefineMethod("TestMethod", MethodAttributes.Public, typeof(int), new Type[] { typeof(int) });
            var mgen = meth.GetILGenerator();
            mgen.Emit(opcodes.Ldarg_0);
            mgen.Emit(opcodes.Ldfld, fbNumber);
            mgen.Emit(opcodes.Ldarg_1);
            mgen.Emit(opcodes.Mul);
            mgen.Emit(opcodes.Ret);

            Type t = tb.CreateType();

            ab.CreateInstance(t.Name);
            return t;
        }
    }
}
