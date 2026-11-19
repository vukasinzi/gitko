namespace snap.CLI;

public static class Repository
{
    private static string? root;

    public static string Root
    {
        get
        {
            if (root == null)
                root = FindRoot();
            return root;
        }
    }

    public static string SnapDir
    {
        get { return Path.Combine(Root, ".snap"); }
    }

    public static string ObjectsDir
    {
        get { return Path.Combine(SnapDir, "objects"); }
    }

    public static string IndexPath
    {
        get { return Path.Combine(SnapDir, "index"); }
    }

    public static string HeadPath
    {
        get { return Path.Combine(SnapDir, "HEAD"); }
    }

    public static string HeadsDir
    {
        get { return Path.Combine(SnapDir, "refs", "heads"); }
    }

    public static string SaltPath
    {
        get { return Path.Combine(SnapDir, "salt"); }
    }

    public static string KeyCheckPath
    {
        get { return Path.Combine(SnapDir, "keycheck"); }
    }

    public static string FullPath(string arg)
    {
        string combined = Path.Combine(Directory.GetCurrentDirectory(), arg);
        return Path.GetFullPath(combined);
    }

    public static string RelativePath(string fullPath)
    {
        return Path.GetRelativePath(Root, fullPath).Replace("\\", "/");
    }

    public static bool Contains(string path)
    {
        string rootPath = Path.GetFullPath(Root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        string fullPath = Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return string.Equals(fullPath, rootPath, comparison) ||
               fullPath.StartsWith(rootPath + Path.DirectorySeparatorChar, comparison) ||
               fullPath.StartsWith(rootPath + Path.AltDirectorySeparatorChar, comparison);
    }

    private static string FindRoot()
    {
        string? currentDir = Directory.GetCurrentDirectory();

        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".snap")))
                return currentDir;

            currentDir = Path.GetDirectoryName(currentDir);
        }

        throw new SnapException("Ne mogu naći root direktorijum!");
    }
}
