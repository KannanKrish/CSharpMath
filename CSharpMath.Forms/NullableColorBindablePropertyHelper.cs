using Xamarin.Forms;
namespace CSharpMath.Forms {
  public static class NullableColorBindablePropertyHelper {
    public static readonly Color Null = Color.Transparent;
    public static Color? GetNullableColor(this BindableObject bindableObject, BindableProperty bindableProperty) {
      var v = (Color)bindableObject.GetValue(bindableProperty);
      return v == Null ? null : v;
    }
    public static void SetNullableColor(this BindableObject bindableObject, BindableProperty bindableProperty, Color? value) => bindableObject.SetValue(bindableProperty, value ?? Null);
  }
}