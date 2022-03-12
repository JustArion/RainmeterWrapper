namespace Dawn.Rainmeter
{
    using System.Collections.Generic;
    using global::Rainmeter;

    public abstract partial class BaseWrapper 
    {
        private static readonly HashSet<BaseWrapper> InstanceContainer = new();
        public API API { get; private set; }

        public virtual void Reload(ref double maxValue) {}

        public virtual double Update() => 0;

        public virtual string GetString() => null;
        
        /// <summary>
        /// 'ExecuteBang'
        /// https://docs.rainmeter.net/developers/plugin/csharp/
        /// </summary>
        public virtual void CommandReceived(string args) {}

        private int ID => GetHashCode();
    }
}