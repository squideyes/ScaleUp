namespace ScaleUp;

public static class MiscExtenders
{
    public static bool IsGuid(this string value) =>
        Guid.TryParse(value, out _);

    public static string WithTrailingSlash(this string value)
    {
        return value.EndsWith(Path.DirectorySeparatorChar.ToString()) ?
            value : value + Path.DirectorySeparatorChar;
    }

    public static bool IsImageFile(this string fileName)
    {
        return Path.GetExtension(fileName).ToLower() switch
        {
            ".jpg" => true,
            ".png" => true,
            _ => false
        };
    }

    public static bool IsThreads(this string threads)
    {
        if (!int.TryParse(threads, out int mdop))
            return false;

        return mdop == -1 || (mdop >= 1 && mdop <= Environment.ProcessorCount);
    }

    public static bool EnsurePathExists(this string value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentOutOfRangeException(nameof(value));

            var path = Path.GetFullPath(value.WithTrailingSlash());

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsFolderName(this string value, bool mustBeRooted = true)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            _ = new DirectoryInfo(value);

            if (!mustBeRooted)
                return true;
            else
                return Path.IsPathRooted(value);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (PathTooLongException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }
}