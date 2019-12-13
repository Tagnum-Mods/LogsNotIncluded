using Harmony;
using KMod;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace LogsNotIncluded.Patches
{
	[HarmonyPatch]
	[HarmonyPriority(1000)]
	static class PDLLLoader_LoadDLLS
	{
		public static NLog.Logger logger
		{
			get
			{
				return Loggers.DLLLoader;
			}
		}

		private static bool patched = false;

		static MethodInfo TargetMethod() { return AccessTools.Method(AccessTools.TypeByName("KMod.DLLLoader"), "LoadDLLs"); }

		static bool Prefix(ref bool __result, string path)
		{
			patched = true;
			logger.Info("Started looking for DLLs in {path}", path);
			__result = LoadDLLs(path);
			return false;
		}

		static void Postfix(string path)
		{
			if (patched)
			{
				logger.Info("Finished looking for DLLs in {path}", path);
			}
		}

		private static bool LoadDLLs(string path)
		{
			try
			{
				if (Testing.dll_loading == Testing.DLLLoading.Fail || Testing.dll_loading == Testing.DLLLoading.UseModLoaderDLLExclusively)
				{
					return false;
				}

				var directoryInfo = new DirectoryInfo(path);

				if (!directoryInfo.Exists) return false;

				var assemblies = new List<Assembly>();
				var files = directoryInfo.GetFiles();

				foreach (FileInfo file in files)
				{
					if (file.Name.ToLower().EndsWith(".dll"))
					{
						logger.Info("Loading Mod DLL: {file_name}", file.Name);
						Assembly assembly = Assembly.LoadFrom(file.FullName);
						if (assembly != null)
						{
							assemblies.Add(assembly);
						}
					}
				}

				if (assemblies.Count == 0) return false;

				ListPool<MethodInfo, Manager>.PooledList pre_patch_methods = ListPool<MethodInfo, Manager>.Allocate();
				ListPool<MethodInfo, Manager>.PooledList post_patch_methods = ListPool<MethodInfo, Manager>.Allocate();
				ListPool<MethodInfo, Manager>.PooledList on_load_method_without_paremeters = ListPool<MethodInfo, Manager>.Allocate();
				ListPool<MethodInfo, Manager>.PooledList on_load_method_with_string_parameter = ListPool<MethodInfo, Manager>.Allocate();

				var no_parameters = new Type[0];
				var string_parameter = new Type[1] { typeof(string) };
				var harmony_instance_parameter = new Type[1] { typeof(HarmonyInstance) };

				MethodInfo methodInfo = null;
				foreach (Assembly assembly in assemblies)
				{
					var types = assembly.GetTypes();
					foreach (Type type in types)
					{
						if (type != null)
						{
							methodInfo = type.GetMethod("OnLoad", no_parameters);
							if (methodInfo != null)
							{
								on_load_method_without_paremeters.Add(methodInfo);
							}
							methodInfo = type.GetMethod("OnLoad", string_parameter);
							if (methodInfo != null)
							{
								on_load_method_with_string_parameter.Add(methodInfo);
							}
							methodInfo = type.GetMethod("PrePatch", harmony_instance_parameter);
							if (methodInfo != null)
							{
								pre_patch_methods.Add(methodInfo);
							}
							methodInfo = type.GetMethod("PostPatch", harmony_instance_parameter);
							if (methodInfo != null)
							{
								post_patch_methods.Add(methodInfo);
							}
						}
					}
				}

				var harmonyInstance = HarmonyInstance.Create($"OxygenNotIncluded_v{0}.{1}");
				if (harmonyInstance != null)
				{
					var parameters = new object[1] { harmonyInstance };
					foreach (MethodInfo pre_patch in pre_patch_methods)
					{
						logger.Debug("Running PrePatch in {module} from {assembly}", pre_patch.DeclaringType.FullName, pre_patch.Module.Name);
						var watch = Stopwatch.StartNew();
						try
						{
							pre_patch.Invoke(null, parameters);
						}
						finally
						{
							watch.Stop();
						}
						logger.Debug("{module} PrePatch took {time}ms to complete", pre_patch.DeclaringType.FullName, watch.ElapsedMilliseconds);
					}
					foreach (Assembly assembly in assemblies)
					{
						logger.Debug("Patching assembly {assembly}", assembly.GetName());
						var watch = Stopwatch.StartNew();
						try
						{
							harmonyInstance.PatchAll(assembly);
						}
						finally
						{
							watch.Stop();
						}
						logger.Debug("Patching {module} took {time}ms to complete", assembly.GetName(), watch.ElapsedMilliseconds);
					}
					foreach (MethodInfo post_patch in post_patch_methods)
					{
						logger.Debug("Running PostPatch in {module} from {assembly}", post_patch.DeclaringType.FullName, post_patch.Module.Name);
						var watch = Stopwatch.StartNew();
						try
						{
							post_patch.Invoke(null, parameters);
						}
						finally
						{
							watch.Stop();
						}
						logger.Debug("{module} PostPatch took {time}ms to complete", post_patch.DeclaringType.FullName, watch.ElapsedMilliseconds);
					}
				}
				pre_patch_methods.Recycle();
				post_patch_methods.Recycle();

				foreach (MethodInfo on_load in on_load_method_without_paremeters)
				{
					logger.Debug("Running OnLoad in {module} from {assembly}", on_load.DeclaringType.FullName, on_load.Module.Name);
					var watch = Stopwatch.StartNew();
					try
					{
						on_load.Invoke(null, null);
					}
					finally
					{
						watch.Stop();
					}
					logger.Debug("{module} OnLoad took {time}ms to complete", on_load.DeclaringType.FullName, watch.ElapsedMilliseconds);
				}
				on_load_method_without_paremeters.Recycle();

				object[] path_parameter = new object[1] { path };
				foreach (MethodInfo on_load in on_load_method_with_string_parameter)
				{
					logger.Debug("Running OnLoad(path) in {module} from {assembly}", on_load.DeclaringType.FullName, on_load.Module.Name);
					var watch = Stopwatch.StartNew();
					try
					{
						on_load.Invoke(null, path_parameter);
					}
					finally
					{
						watch.Stop();
					}
					logger.Debug("{module} OnLoad(path) took {time}ms to complete", on_load.DeclaringType.FullName, watch.ElapsedMilliseconds);
				}
				on_load_method_with_string_parameter.Recycle();

				return true;
			}
			catch (Exception e)
			{
				logger.Error(e, "Failed to load mod from {path}", path);
				return false;
			}
		}
	}
}