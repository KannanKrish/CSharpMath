namespace CSharpMath.MAUI;

public static class MathKeyboardExtensions {
  public static void BindDisplay(this MathKeyboard keyboard,
    SKCanvasView view, MathPainter settings, SKColor caretColor,
    CaretShape caretShape = CaretShape.IBeam) {
    view.EnableTouchEvents = true;
    view.Touch +=
      (sender, e) => {
        if (e.ActionType == SKTouchAction.Pressed)
          keyboard.MoveCaretToPoint(new System.Drawing.PointF(e.Location.X, e.Location.Y));
      };
    keyboard.RedrawRequested += (_, __) => view.InvalidateSurface();
    view.PaintSurface +=
      (sender, e) => {
        var c = e.Surface.Canvas;
        c.Clear();
        MathPainter.DrawDisplay(settings, keyboard.Display, c);
        if (keyboard.ShouldDrawCaret)
          keyboard.DrawCaret(new SkiaCanvas(c, settings.AntiAlias), caretColor.FromNative(), caretShape);
      };
  }
}