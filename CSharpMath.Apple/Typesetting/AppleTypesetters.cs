﻿using System;
using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using Foundation;
using CoreText;
using TGlyph = System.UInt16;
using CSharpMath.Ios.Resources;

namespace CSharpMath.Apple {
  public static class AppleTypesetters {
    public static TypesettingContext<AppleMathFont, TGlyph> CreateTypesettingContext(CTFont someCtFontSizeIrrelevant) {
      var glyphFinder = new CtFontGlyphFinder(someCtFontSizeIrrelevant);
      return new TypesettingContext<AppleMathFont, TGlyph>(
        new AppleFontMeasurer(),
        (font, size) => new AppleMathFont(font.Name, size),
        new AppleGlyphBoundsProvider(glyphFinder),
        new AppleGlyphNameProvider(someCtFontSizeIrrelevant),
        glyphFinder,
        new UnicodeFontChanger(),
        IosResources.LatinMath
      );
    }

    public static TypesettingContext<AppleMathFont, TGlyph> CreateLatinMath() {
      var fontSize = 40;
      var appleFont = new AppleMathFont("latinmodern-math", fontSize);
      return CreateTypesettingContext(appleFont.CtFont);
    }
  }
}
