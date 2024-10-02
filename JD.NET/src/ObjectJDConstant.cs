namespace JDSpace
{
    /// <summary>
    /// Right hand side - double.
    /// </summary>
    internal class ObjectJDConstant : JdConstant
    {
        public override int Numel
        {
            get { return 1; }
        }
        public override object this[int i]
        {
            get { return _val; }
        }
        private object _val;
        internal ObjectJDConstant(object val, ScLinExprFactory scFactory)
            : base(scFactory)
        {
            _val = val;
        }
    }
}