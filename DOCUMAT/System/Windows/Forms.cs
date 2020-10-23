namespace System.Windows
{
    public class Forms
    {
        internal interface IWin32Window
        {
            IntPtr Handle { get; }
        }
    }
}