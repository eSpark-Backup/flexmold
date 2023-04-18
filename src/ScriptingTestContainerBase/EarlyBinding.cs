namespace ScriptingTest
{
    /// <summary>
    /// Configuration Factory for Early Binding
    /// </summary>
    /// <remarks>DTE and Solution objects are stored in static types.</remarks>
    public class EarlyBoundFactory : ConfigurationFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EarlyBoundFactory"/> class.
        /// </summary>
        public EarlyBoundFactory(IVsFactory factory, bool useComRegistration)
            : base(factory, useComRegistration)
        {
        }
    }
}
