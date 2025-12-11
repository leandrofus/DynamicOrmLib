using System.Text.RegularExpressions;

namespace DynamicOrmLib;

public static class SqlProtection
{
  private static readonly Regex ValidIdentifier = new(@"^[A-Za-z0-9_.]+$");

  public static string ValidateIdentifier(string id, string nameForError = "identifier")
  {
    if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException($"{nameForError} must not be empty");
    if (!ValidIdentifier.IsMatch(id)) throw new ArgumentException($"{nameForError} contains invalid characters: {id}");
    return id;
  }

  public static string ValidateModelName(string modelName) => ValidateIdentifier(modelName, "model name");
  public static string ValidateFieldName(string fieldName) => ValidateIdentifier(fieldName, "field name");
  public static string SanitizeIndexName(string modelName, string fieldName)
  {
    var mn = modelName.Replace('.', '_').Replace('-', '_');
    var fn = fieldName.Replace('.', '_').Replace('-', '_');
    return $"idx_{mn}_{fn}";
  }
}
