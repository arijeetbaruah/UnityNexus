namespace Baruah.Nexus.Exception
{
    public sealed class NoBindingException : System.Exception
    {
        public NoBindingException(System.Type type) : base(($"[DI] No binding for {type.Name}"))
        {
        }
    }
}
