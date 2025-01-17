using Xamarin.Forms;

namespace CSharpMath.Forms {
  public static class LatexHelper {
    public static readonly string phantom = SetColor("|", Color.Transparent);
    public static string SetColor(string latex, Color? color) => color != null ? @"\color{" + color.Value.ToHex() + "}{" + latex + "}" : latex;
  }
}