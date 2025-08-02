namespace EzDualMatch
{
    internal class Global
    {
        public static string AppPathRoot
        {
            get => @"D:\AUTOMATION\Eazy Dual Match";
        }
        public static string AppPath(string folder = null, string folder2 = null)
        {
            if (string.IsNullOrEmpty(folder))
                return AppPathRoot;
            else if (string.IsNullOrEmpty(folder2))
                return System.IO.Path.Combine(AppPathRoot, folder);
            else
                return System.IO.Path.Combine(AppPathRoot, folder, folder2);
        }
    }
}
