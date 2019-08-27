using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace CoreCloud
{
	public class ParallelObject<I>
		where I: class
	{
		#region instance-related
		public static implicit operator I(ParallelObject<I> obj)
		{
			return (I)obj;
		}
		#endregion instance-related

		#region scheduling
		protected readonly List<RemoteInstance<I>> instances = new List<RemoteInstance<I>>();
		readonly Random random = new Random();

		protected I Schedule()
		{
			if (instances.Count == 0)
				throw new InvalidOperationException("No instance available");

			return instances.ElementAt(random.Next(instances.Count)).Instance;
		}
		#endregion scheduling

		#region construction
		public static I Create<T>()
			where T: I, new()
		{
			var interfaceType = typeof(I);
			if (!interfaceType.IsInterface)
				throw new InvalidOperationException("You must specify an interface type while providing type parameter I");

			if (!interfaceType.IsPublic && !interfaceType.IsNestedPublic)
				throw new InvalidOperationException("Interface type must be public");

			if (!typeof(T).IsPublic && !typeof(T).IsNestedPublic)
				throw new InvalidOperationException("You must specify public type while providing type parameter T");

			var result = Activator.CreateInstance(parallelObjectType) as ParallelObject<I>;
			result.CreateRemote<T>();
			return result as I;
		}

		private void CreateRemote<T>()
			where T: I, new()
		{
			var remote = ParallelObjects.CreateRemote<T, I>();

			this.instances.Add(new RemoteInstance<I>(remote));
		}

		private void CreateLocal<T>()
			where T: I, new()
		{
		 	this.instances.Add(new RemoteInstance<I>(new T()));
		}
		#endregion construction

		readonly static Type parallelObjectType;

		static ParallelObject()
		{
			var builder = ParallelObjects.dynamicModule.DefineType(typeof(I).FullName,
				TypeAttributes.Public, typeof(ParallelObject<I>));

			builder.AddInterfaceImplementation(typeof(I));

			foreach (var method in typeof(I).GetMethods())
			{
				if (method.ReturnType == null || method.ReturnType == typeof(void))
					DefineModifyMethodProxy(builder, method);
				else
					DefineCalculateMethodProxy(builder, method);
			}

			parallelObjectType = builder.CreateType();
		}

		private static void DefineModifyMethodProxy(TypeBuilder builder, MethodInfo method)
		{
			var parameters = method.GetParameters();

			if (parameters.Any(par => par.IsOut))
				throw new ArgumentException("Method must not have any out parameters", "method");

			if (method.ReturnType != typeof(void) &&
				method.ReturnType != null)
				throw new ArgumentException("Method must not have any return type", "method");

			var result = builder.DefineMethod(method.Name,
				MethodAttributes.Public | MethodAttributes.Virtual,
				method.ReturnType,
				parameters.Select(par => par.ParameterType).ToArray());

			var il = result.GetILGenerator();
			var index = il.DeclareLocal(typeof(int));
			// index.SetLocalSymInfo("index");

			il.Emit(OpCodes.Ldarg_0);
			// 
			var instances = typeof(ParallelObject<I>).GetField("instances", BindingFlags.NonPublic | BindingFlags.Instance);
			if (instances == null)
				throw new InvalidProgramException();
			il.Emit(OpCodes.Ldfld, instances);
			// this.instances
			var count = typeof(List<RemoteInstance<I>>).GetProperty("Count");
			if (count == null)
				throw new InvalidProgramException();
			il.Emit(OpCodes.Call, count.GetGetMethod());
			// .Count
			il.Emit(OpCodes.Ldc_I4_M1);
			il.Emit(OpCodes.Add);
			il.Emit(OpCodes.Stloc, index);
			// i = this.instances.Count + -1

			var loop = il.DefineLabel();
			il.MarkLabel(loop);
			// loop:

			var exit = il.DefineLabel();

			il.Emit(OpCodes.Ldloc, index);
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Blt, exit);
			// if (i < 0) goto exit;

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, instances);
			// this.instances
			il.Emit(OpCodes.Ldloc, index);
			var item = typeof(List<RemoteInstance<I>>).GetProperty("Item");
			if (item == null)
				throw new InvalidProgramException();
			il.Emit(OpCodes.Call, item.GetGetMethod());
			// [i]
			var instance = typeof(RemoteInstance<I>).GetProperty("Instance");
			if (instance == null)
				throw new InvalidProgramException();
			il.Emit(OpCodes.Call, instance.GetGetMethod());
			// .Instance

			EmitLoadParametersThis(parameters, il);
			il.Emit(OpCodes.Call, method);
			// .Call(args)

			il.Emit(OpCodes.Ldloc, index);
			il.Emit(OpCodes.Ldc_I4_M1);
			il.Emit(OpCodes.Add);
			il.Emit(OpCodes.Stloc, index);
			// i += -1

			il.Emit(OpCodes.Br, loop);

			il.MarkLabel(exit);
			il.Emit(OpCodes.Ret);

			// builder.DefineMethodOverride(result, method);
		}

		private static void DefineCalculateMethodProxy(TypeBuilder builder, MethodInfo method)
		{
			var parameters = method.GetParameters();

			var result = builder.DefineMethod(method.Name,
				MethodAttributes.Public | MethodAttributes.Virtual,
				method.ReturnType,
				parameters.Select(param => param.ParameterType).ToArray());

			var il = result.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, typeof(ParallelObject<I>).GetMethod("Schedule", 
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

			EmitLoadParametersThis(parameters, il);

			il.Emit(OpCodes.Call, method);
			il.Emit(OpCodes.Ret);

			// builder.DefineMethodOverride(result, method);
		}

		private static void EmitLoadParametersThis(ParameterInfo[] parameters, ILGenerator il)
		{
			for (int i = 0; i < parameters.Length; i++)
			{
				switch (i)
				{
				case 0:
					il.Emit(OpCodes.Ldarg_1);
					break;

				case 1:
					il.Emit(OpCodes.Ldarg_2);
					break;

				case 2:
					il.Emit(OpCodes.Ldarg_3);
					break;

				default:
					var paramnum = i + 1;
					if (i < 255)
						il.Emit(OpCodes.Ldarg_S, (byte)paramnum);
					else
						il.Emit(OpCodes.Ldarg, paramnum);
					break;
				}
			}
		}
	}
}
