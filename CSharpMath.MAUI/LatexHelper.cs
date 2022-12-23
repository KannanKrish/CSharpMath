namespace CSharpMath.MAUI; 

public static class LatexHelper {
  public static readonly string phantom = SetColor("|", Color.FromRgba(0, 0, 0, 0));
  public static string SetColor(string latex, Color? color) => color != null ? @"\color{" + color.ToHex() + "}{" + latex + "}" : latex;
}