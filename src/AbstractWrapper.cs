namespace Dawn.Rainmeter
{
    using System;

    public abstract class AbstractWrapper : BaseWrapper
    {
        private string _stringCache;
        private double _doubleCache;
        

        /// <typeparam name="T"><see cref="string"/> / <see cref="double"/></typeparam>
        /// Returning Twice only returns the most recent value.
        protected void Return<T>(T value) where T : notnull
        {
            if (typeof(T) == typeof(string))
            {
                _stringCache = Convert.ToString(value);
                _doubleCache = 0;
            }
            
            //IConvertible allows for small value types to be cast up. int -> double, byte -> double, etc
            else if (typeof(T).IsValueType && value is IConvertible)
            {
                _doubleCache = Convert.ToDouble(value);
                _stringCache = null;
            }
            else
            {
                _stringCache = value?.ToString();
                _doubleCache = 0;
            }
        }
        
        protected virtual void OnUpdate() {}

        public sealed override string GetString() => _stringCache;

        public sealed override double Update()
        {
            OnUpdate();
            return _doubleCache;
        }
    }
}