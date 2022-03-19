namespace Dawn.Rainmeter
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using global::Rainmeter;
    using Rainmeter;

    public abstract partial class BaseWrapper
    {
        public IntPtr Pointer { get; private set; }
        
        [DllExport] // ===========================================================
        private static void Initialize(ref IntPtr data, IntPtr rm)
        {

            var instanceID = InjectInstance(rm);
            if (instanceID == -1) return;
            TryGetInstance(instanceID, out var instance); // We know it's not null since the ID is valid.
            data = GCHandle.ToIntPtr(GCHandle.Alloc(instanceID)); // Possible expansion of storing meta-data into the pointer too such as number of reloads, and the instance itself. Hashtable or something.
            instance.Pointer = data;
        }

        private static int InjectInstance(API meter)
        {
            var pluginType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => typeof(BaseWrapper).IsAssignableFrom(t) && !t.IsAbstract);
            if (pluginType == null)
                return -1;
            try
            {
                var pluginInstance = (BaseWrapper)Activator.CreateInstance(pluginType);
                pluginInstance.API = meter;
                InstanceContainer.Add(pluginInstance);
                return pluginInstance.ID;
            }
            catch {}

            return -1;
        }
        private static bool TryGetInstance(IntPtr ptr, out BaseWrapper instance)
        {
            try
            {
                var instanceID = (int)GCHandle.FromIntPtr(ptr).Target;
                instance = InstanceContainer.FirstOrDefault(i => i.ID == instanceID);
                return instance != null;
            }
            catch // Catch is mainly for if the object cast fails.
            {
                instance = null;
                return false;
            }
        }
        private static bool TryGetInstance(int instanceID, out BaseWrapper instance)
        {
            try
            {
                instance = InstanceContainer.FirstOrDefault(i => i.ID == instanceID);
                return instance != null;
            }
            catch // Catch is mainly for if the object cast fails.
            {
                instance = null;
                return false;
            }
        }
        
        [DllExport] // ===========================================================
        private static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            if (!TryGetInstance(data, out var instance)) return;
            instance.API = rm;
            instance.ExceptionWrapper(ref maxValue);
        }

        [DllExport] // ===========================================================
        private static double Update(IntPtr data)
        {
            if (!TryGetInstance(data, out var instance)) return 0;
            return instance.ExceptionWrapper(instance.Update);
        }

        //The buffer seems to serve the purpose of holding the string data as the target for disposal later
        //simply initializing the pointer and returning it will cause disposal to not occur when finalize is called.
        private IntPtr _StringBufferPtr;
        [DllExport] // ===========================================================
        private static IntPtr GetString(IntPtr data)
        {
            if (!TryGetInstance(data, out var instance)) return IntPtr.Zero;
            
            if (instance._StringBufferPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(instance._StringBufferPtr);
                instance._StringBufferPtr = IntPtr.Zero;
            }

            var stringValue = instance.ExceptionWrapper(instance.GetString);
            if (stringValue != null) 
                instance._StringBufferPtr = Marshal.StringToHGlobalUni(stringValue);

            return instance._StringBufferPtr;
        }
        
        [DllExport]
        private static void ExecuteBang(IntPtr data, [MarshalAs(UnmanagedType.LPWStr)] string args)
        {
            if (!TryGetInstance(data, out var instance)) return;
            instance.ExceptionWrapper(()=> instance.CommandReceived(args));
        }

        [DllExport] // ===========================================================
        private static void Finalize(IntPtr data)
        {
            if (!TryGetInstance(data, out var instance)) 
                return;
            try { AppDomain.CurrentDomain.UnhandledException -= instance.RaiseUnhandledException; }
            catch {} // To prevent the exception from being thrown if the exception handler is already removed.
            
            try
            {
                if (instance._StringBufferPtr == IntPtr.Zero) return;
                Marshal.FreeHGlobal(instance._StringBufferPtr);
                instance._StringBufferPtr = IntPtr.Zero;
            }
            finally
            {
                InstanceContainer.Remove(instance);
                GCHandle.FromIntPtr(data).Free();
            }
        }
        
        #region  Exception Wrappers

        protected BaseWrapper() => AppDomain.CurrentDomain.UnhandledException += RaiseUnhandledException;

        /// <summary>
        /// If a fatal exception occurs, this will be called, the Rainmeter program may be terminated if 'Fatal'.
        /// We lower the chances of Fatal exceptions by employing Exception Wrappers
        /// </summary>
        private void RaiseUnhandledException(object sender, UnhandledExceptionEventArgs e) 
            => API?.Log(API.LogType.Error, $"[{(e.IsTerminating ? "Fatal" : "Unhandled")}] Exception: {e.ExceptionObject as Exception}");

        private void ExceptionWrapper(Action act)
        {
            try { act(); }
            catch (Exception e)
            {
                RaiseException(e);
            }
        }
        private T ExceptionWrapper<T>(Func<T> func)
        {
            try { return func(); }
            catch (Exception e)
            {
                RaiseException(e);
                return default;
            }
        }

        private void RaiseException(Exception e) => API?.Log(API.LogType.Error, $"Unhandled Exception: {e}");
        private void ExceptionWrapper(ref double maxValue)
        {
            try
            {
                Reload(ref maxValue);
            }
            catch (Exception e)
            {
                RaiseException(e);
            }
        }

        #endregion
    }
}
