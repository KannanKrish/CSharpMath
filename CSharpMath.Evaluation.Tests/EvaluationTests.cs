using System.Linq;
using Xunit;
using AngouriMath;

namespace CSharpMath {
  using Atom;
  public class EvluationTests {
    MathList ParseLaTeX(string latex) =>
      LaTeXParser.MathListFromLaTeX(latex).Match(list => list, e => throw new Xunit.Sdk.XunitException(e));
    Entity ParseMath(string latex) =>
      Evaluation.MathListToEntity(ParseLaTeX(latex)).Match(entity => entity, e => throw new Xunit.Sdk.XunitException(e));
    void Test(string input, string converted, string result) {
      var math = ParseMath(input);
      Assert.Equal(converted, LaTeXParser.MathListToLaTeX(Evaluation.MathListFromEntity(math)).ToString());
      // Ensure that the converted entity is valid by simplifying it
      Assert.Equal(result, LaTeXParser.MathListToLaTeX(Evaluation.MathListFromEntity(math.Simplify())).ToString());
    }
    [Theory]
    [InlineData("1")]
    [InlineData("1234")]
    [InlineData("1234.", "1234")]
    [InlineData(".5678", "0.5678")]
    [InlineData("1234.5678")]
    public void Numbers(string number, string? output = null) =>
      Test(number, output ?? number, output ?? number);
    [Theory]
    [InlineData("a", "a", "a")]
    [InlineData("ab", @"a\times b", @"a\times b")]
    [InlineData(@"\alpha", @"α", "α")]
    [InlineData(@"\pi", @"pi", @"pi")]
    [InlineData(@"\alpha\pi", @"α\times pi", @"pi\times α")]
    [InlineData("abc", @"a\times b\times c", @"a\times b\times c")]
    [InlineData("3a", @"3\times a", @"3\times a")]
    [InlineData("3ab", @"3\times a\times b", @"3\times a\times b")]
    [InlineData("3a3", @"3\times a\times 3", @"9\times a")]
    [InlineData("3aa", @"3\times a\times a", @"3\times a^2")]
    public void Variables(string input, string converted, string result) => Test(input, converted, result);
    [Theory]
    [InlineData("a + b", @"a+b", "a+b")]
    [InlineData("a - b", @"a-b", "a-b")]
    [InlineData("a * b", @"a\times b", @"a\times b")]
    [InlineData(@"a\times b", @"a\times b", @"a\times b")]
    [InlineData(@"a\cdot b", @"a\times b", @"a\times b")]
    [InlineData(@"a / b", @"\frac{a}{b}", @"\frac{a}{b}")]
    [InlineData(@"a\div b", @"\frac{a}{b}", @"\frac{a}{b}")]
    [InlineData(@"\frac ab", @"\frac{a}{b}", @"\frac{a}{b}")]
    [InlineData("a + b + c", @"a+b+c", "a+b+c")]
    [InlineData("a + b - c", @"a+b-c", "a+b-c")]
    [InlineData("a + b * c", @"a+b\times c", @"a+b\times c")]
    [InlineData("a + b / c", @"a+\frac{b}{c}", @"a+\frac{b}{c}")]
    [InlineData("a - b + c", @"a-b+c", "a+c-b")]
    [InlineData("a - b - c", @"a-b-c", @"a-\left( b+c\right) ")]
    [InlineData("a - b * c", @"a-b\times c", @"a-b\times c")]
    [InlineData("a - b / c", @"a-\frac{b}{c}", @"a-\frac{b}{c}")]
    [InlineData("a * b + c", @"a\times b+c", @"a\times b+c")]
    [InlineData("a * b - c", @"a\times b-c", @"a\times b-c")]
    [InlineData("a * b * c", @"a\times b\times c", @"a\times b\times c")]
    [InlineData("a * b / c", @"\frac{a\times b}{c}", @"\frac{a\times b}{c}")]
    [InlineData("a / b + c", @"\frac{a}{b}+c", @"\frac{a}{b}+c")]
    [InlineData("a / b - c", @"\frac{a}{b}-c", @"\frac{a}{b}-c")]
    [InlineData("a / b * c", @"\frac{a}{b}\times c", @"\frac{a\times c}{b}")]
    [InlineData("a / b / c", @"\frac{\frac{a}{b}}{c}", @"\frac{\frac{a}{b}}{c}")]
    [InlineData(@"2+\frac ab", @"2+\frac{a}{b}", @"2+\frac{a}{b}")]
    [InlineData(@"\frac ab+2", @"\frac{a}{b}+2", @"2+\frac{a}{b}")]
    [InlineData(@"2-\frac ab", @"2-\frac{a}{b}", @"2-\frac{a}{b}")]
    [InlineData(@"\frac ab-2", @"\frac{a}{b}-2", @"\frac{a}{b}-2")]
    [InlineData(@"2*\frac ab", @"2\times \frac{a}{b}", @"\frac{2\times a}{b}")]
    [InlineData(@"\frac ab*2", @"\frac{a}{b}\times 2", @"\frac{2\times a}{b}")]
    [InlineData(@"2/\frac ab", @"\frac{2}{\frac{a}{b}}", @"\frac{2\times b}{a}")]
    [InlineData(@"\frac ab/2", @"\frac{\frac{a}{b}}{2}", @"\frac{\frac{a}{2}}{b}")]
    public void BinaryOperators(string latex, string converted, string result) => Test(latex, converted, result);
    [Theory]
    [InlineData("+a", "a", "a")]
    [InlineData("-a", "-a", "-a")]
    [InlineData("++a", "a", "a")]
    [InlineData("+-a", "-a", "-a")]
    [InlineData("-+a", "-a", "-a")]
    [InlineData("--a", "--a", "a")]
    [InlineData("+++a", "a", "a")]
    [InlineData("---a", "---a", "-a")]
    [InlineData("a++a", "a+a", @"2\times a")]
    [InlineData("a+-a", "a+-a", "0")]
    [InlineData("a-+a", "a-a", "0")]
    [InlineData("a--a", "a--a", @"2\times a")]
    [InlineData("a+++a", "a+a", @"2\times a")]
    [InlineData("a---a", "a---a", "0")]
    [InlineData("a*+a", @"a\times a", "a^2")]
    [InlineData("a*-a", @"a\times -a", "-a^2")]
    [InlineData("+a*+a", @"a\times a", "a^2")]
    [InlineData("-a*-a", @"-a\times -a", "a^2")]
    [InlineData("a/+a", @"\frac{a}{a}", "1")]
    [InlineData("a/-a", @"\frac{a}{-a}", "-1")]
    [InlineData("+a/+a", @"\frac{a}{a}", "1")]
    [InlineData("-a/-a", @"\frac{-a}{-a}", "1")]
    [InlineData("-2+-2+-2", @"-2+-2+-2", "-6")]
    [InlineData("-2--2--2", @"-2--2--2", "2")]
    [InlineData("-2*-2*-2", @"-2\times -2\times -2", "-8")]
    [InlineData("-2/-2/-2", @"\frac{\frac{-2}{-2}}{-2}", "-0.5")]
    public void UnaryOperators(string latex, string converted, string result) => Test(latex, converted, result);
    [Theory]
    [InlineData(@"9\%", @"\frac{9}{100}", "0.09")]
    [InlineData(@"a\%", @"\frac{a}{100}", @"0.01\times a")]
    [InlineData(@"a\%\%", @"\frac{\frac{a}{100}}{100}", @"0.0001\times a")]
    [InlineData(@"9\%+3", @"\frac{9}{100}+3", "3.09")]
    [InlineData(@"-9\%+3", @"-\frac{9}{100}+3", "2.91")]
    [InlineData(@"2^2\%", @"\frac{2^2}{100}", "0.04")]
    [InlineData(@"2\%^2", @"\left( \frac{2}{100}\right) ^2", "0.0004")]
    [InlineData(@"2\%2", @"\frac{2}{100}\times 2", "0.04")]
    [InlineData(@"1+2\%^2", @"1+\left( \frac{2}{100}\right) ^2", "1.0004")]
    public void PostfixOperators(string latex, string converted, string result) => Test(latex, converted, result);
    [Theory]
    [InlineData("2^2", "2^2", "4")]
    [InlineData(".2^2", "0.2^2", "0.04000000000000001")]
    [InlineData("2.^2", "2^2", "4")]
    [InlineData("2.1^2", "2.1^2", "4.41")]
    [InlineData("a^a", "a^a", "a^a")]
    [InlineData("a^{a+b}", "a^{a+b}", "a^{a+b}")]
    [InlineData("a^{-2}", "a^{-2}", "a^{-2}")]
    [InlineData("2^{3^4}", "2^{3^4}", "2.4178516392292583E+24")]
    [InlineData("4^{3^2}", "4^{3^2}", "262144")]
    [InlineData("4^3+2", "4^3+2", "66")]
    [InlineData("2+3^4", "2+3^4", "83")]
    [InlineData("4^3*2", @"4^3\times 2", "128")]
    [InlineData("2*3^4", @"2\times 3^4", "162")]
    [InlineData("1/x", @"\frac{1}{x}", @"x^{-1}")]
    [InlineData("2/x", @"\frac{2}{x}", @"\frac{2}{x}")]
    [InlineData("0^x", @"0^x", @"0")]
    [InlineData("1^x", @"1^x", @"1")]
    [InlineData("x^0", @"x^0", @"1")]
    [InlineData(@"{\frac 12}^4", @"\left( \frac{1}{2}\right) ^4", "0.0625")]
    [InlineData(@"\sqrt2", @"\sqrt{2}", "1.4142135623730951")]
    [InlineData(@"\sqrt2^2", @"\left( \sqrt{2}\right) ^2", "2.0000000000000004")]
    [InlineData(@"\sqrt[3]2", @"2^{\frac{1}{3}}", "1.2599210498948732")]
    [InlineData(@"\sqrt[3]2^3", @"\left( 2^{\frac{1}{3}}\right) ^3", "2")]
    [InlineData(@"\sqrt[3]2^{1+1+1}", @"\left( 2^{\frac{1}{3}}\right) ^{1+1+1}", "2")]
    [InlineData(@"\sqrt[1+1+1]2^{1+1+1}", @"\left( 2^{\frac{1}{1+1+1}}\right) ^{1+1+1}", "2")]
    public void Exponents(string latex, string converted, string result) => Test(latex, converted, result);
    [Theory]
    [InlineData(@"\sin x", @"\sin \left( x\right) ", @"\sin \left( x\right) ")]
    [InlineData(@"\cos x", @"\cos \left( x\right) ", @"\cos \left( x\right) ")]
    [InlineData(@"\tan x", @"\tan \left( x\right) ", @"\tan \left( x\right) ")]
    [InlineData(@"\cot x", @"\cot \left( x\right) ", @"\cot \left( x\right) ")]
    [InlineData(@"\sec x", @"\frac{1}{\cos \left( x\right) }", @"\cos \left( x\right) ^{-1}")]
    [InlineData(@"\csc x", @"\frac{1}{\sin \left( x\right) }", @"\sin \left( x\right) ^{-1}")]
    [InlineData(@"\arcsin x", @"\arcsin \left( x\right) ", @"\arcsin \left( x\right) ")]
    [InlineData(@"\arccos x", @"\arccos \left( x\right) ", @"\arccos \left( x\right) ")]
    [InlineData(@"\arctan x", @"\arctan \left( x\right) ", @"\arctan \left( x\right) ")]
    [InlineData(@"\arccot x", @"\arccot \left( x\right) ", @"\arccot \left( x\right) ")]
    [InlineData(@"\arcsec x", @"\arccos \left( \frac{1}{x}\right) ", @"\arccos \left( x^{-1}\right) ")]
    [InlineData(@"\arccsc x", @"\arcsin \left( \frac{1}{x}\right) ", @"\arcsin \left( x^{-1}\right) ")]
    [InlineData(@"\ln x", @"\ln \left( x\right) ", @"\ln \left( x\right) ")]
    [InlineData(@"\log x", @"\log \left( x\right) ", @"\log \left( x\right) ")]
    [InlineData(@"\log_3 x", @"\log _3\left( x\right) ", @"\log _3\left( x\right) ")]
    [InlineData(@"\log_{10} x", @"\log \left( x\right) ", @"\log \left( x\right) ")]
    [InlineData(@"\log_e x", @"\ln \left( x\right) ", @"\ln \left( x\right) ")]
    [InlineData(@"2\sin x", @"2\times \sin \left( x\right) ", @"2\times \sin \left( x\right) ")]
    [InlineData(@"\sin 2x", @"\sin \left( 2\times x\right) ", @"\sin \left( 2\times x\right) ")]
    [InlineData(@"\sin xy", @"\sin \left( x\times y\right) ", @"\sin \left( x\times y\right) ")]
    [InlineData(@"\cos +x", @"\cos \left( x\right) ", @"\cos \left( x\right) ")]
    [InlineData(@"\cos -x", @"\cos \left( -x\right) ", @"\cos \left( -x\right) ")]
    [InlineData(@"\tan x\%", @"\tan \left( \frac{x}{100}\right) ", @"\tan \left( 0.01\times x\right) ")]
    [InlineData(@"\tan x\%^2", @"\tan \left( \left( \frac{x}{100}\right) ^2\right) ", @"\tan \left( 0.0001\times x^2\right) ")]
    [InlineData(@"\cot x*y", @"\cot \left( x\right) \times y", @"\cot \left( x\right) \times y")]
    [InlineData(@"\cot x/y", @"\frac{\cot \left( x\right) }{y}", @"\frac{\cot \left( x\right) }{y}")]
    [InlineData(@"\cos \arccos x", @"\cos \left( \arccos \left( x\right) \right) ", @"x")]
    [InlineData(@"\sin^2 x", @"\sin \left( x\right) ^2", @"\sin \left( x\right) ^2")]
    [InlineData(@"\sin^2 xy+\cos^2 yx", @"\sin \left( x\times y\right) ^2+\cos \left( y\times x\right) ^2", @"1")]
    [InlineData(@"\log^2 x", @"\log \left( x\right) ^2", @"\log \left( x\right) ^2")]
    [InlineData(@"\ln^2 x", @"\ln \left( x\right) ^2", @"\ln \left( x\right) ^2")]
    [InlineData(@"\log_{10}^2 x", @"\log \left( x\right) ^2", @"\log \left( x\right) ^2")]
    [InlineData(@"\log_3^2 x", @"\log _3\left( x\right) ^2", @"\log _3\left( x\right) ^2")]
    public void LargeOperators(string latex, string converted, string result) => Test(latex, converted, result);
    [Theory]
    [InlineData(@"1+(2+3)", @"1+2+3", @"6")]
    [InlineData(@"1+((2+3))", @"1+2+3", @"6")]
    [InlineData(@"2*(3+4)", @"2\times \left( 3+4\right) ", @"14")]
    [InlineData(@"(3+4)*2", @"\left( 3+4\right) \times 2", @"14")]
    [InlineData(@"(5+6)^2", @"\left( 5+6\right) ^2", @"121")]
    [InlineData(@"(5+6)", @"5+6", @"11")]
    [InlineData(@"((5+6))", @"5+6", @"11")]
    [InlineData(@"(5+6)2", @"\left( 5+6\right) \times 2", @"22")]
    [InlineData(@"2(5+6)", @"2\times \left( 5+6\right) ", @"22")]
    [InlineData(@"2(5+6)2", @"2\times \left( 5+6\right) \times 2", @"44")]
    [InlineData(@"(5+6)x", @"\left( 5+6\right) \times x", @"11\times x")]
    [InlineData(@"x(5+6)", @"x\times \left( 5+6\right) ", @"11\times x")]
    [InlineData(@"x(5+6)x", @"x\times \left( 5+6\right) \times x", @"11\times x^2")]
    [InlineData(@"(5+6).2", @"\left( 5+6\right) \times 0.2", @"2.2")]
    [InlineData(@".2(5+6)", @"0.2\times \left( 5+6\right) ", @"2.2")]
    [InlineData(@".2(5+6).2", @"0.2\times \left( 5+6\right) \times 0.2", @"0.44000000000000006")]
    [InlineData(@"(5+6)2.", @"\left( 5+6\right) \times 2", @"22")]
    [InlineData(@"2.(5+6)", @"2\times \left( 5+6\right) ", @"22")]
    [InlineData(@"2.(5+6)2.", @"2\times \left( 5+6\right) \times 2", @"44")]
    [InlineData(@"(5+6)(2)", @"\left( 5+6\right) \times 2", @"22")]
    [InlineData(@"(5+6)(1+1)", @"\left( 5+6\right) \times \left( 1+1\right) ", @"22")]
    [InlineData(@"(5+6)(-(-2))", @"\left( 5+6\right) \times --2", @"22")]
    [InlineData(@"(5+6)(--2)", @"\left( 5+6\right) \times --2", @"22")]
    [InlineData(@"+(1)", @"1", @"1")]
    [InlineData(@"+(-1)", @"-1", @"-1")]
    [InlineData(@"-(+1)", @"-1", @"-1")]
    [InlineData(@"-(-1)", @"--1", @"1")]
    [InlineData(@"--(--1)", @"----1", @"1")]
    [InlineData(@"(2+3)^{(4+5)}", @"\left( 2+3\right) ^{4+5}", @"1953125")]
    [InlineData(@"(2+3)^{((4)+5)}", @"\left( 2+3\right) ^{4+5}", @"1953125")]
    [InlineData(@"2\sin(x)", @"2\times \sin \left( x\right) ", @"2\times \sin \left( x\right) ")]
    [InlineData(@"(2)\sin(x)", @"2\times \sin \left( x\right) ", @"2\times \sin \left( x\right) ")]
    [InlineData(@"\sin(x+1)", @"\sin \left( x+1\right) ", @"\sin \left( 1+x\right) ")]
    [InlineData(@"\sin((x+1))", @"\sin \left( x+1\right) ", @"\sin \left( 1+x\right) ")]
    [InlineData(@"\sin(2(x+1))", @"\sin \left( 2\times \left( x+1\right) \right) ", @"\sin \left( 2\times \left( 1+x\right) \right) ")]
    [InlineData(@"\sin((x+1)+2)", @"\sin \left( x+1+2\right) ", @"\sin \left( 3+x\right) ")]
    [InlineData(@"\sin(x)2", @"\sin \left( x\right) \times 2", @"2\times \sin \left( x\right) ")]
    [InlineData(@"\sin(x)(x+1)", @"\sin \left( x\right) \times \left( x+1\right) ", @"\sin \left( x\right) \times \left( 1+x\right) ")]
    [InlineData(@"\sin(x)(x+1)(x)", @"\sin \left( x\right) \times \left( x+1\right) \times x", @"\sin \left( x\right) \times \left( 1+x\right) \times x")]
    [InlineData(@"\sin(x^2)", @"\sin \left( x^2\right) ", @"\sin \left( x^2\right) ")]
    [InlineData(@"\sin\ (x^2)", @"\sin \left( x^2\right) ", @"\sin \left( x^2\right) ")]
    [InlineData(@"\sin\; (x^2)", @"\sin \left( x^2\right) ", @"\sin \left( x^2\right) ")]
    [InlineData(@"\sin\ \; (x^2)", @"\sin \left( x^2\right) ", @"\sin \left( x^2\right) ")]
    [InlineData(@"\sin^3(x)", @"\sin \left( x\right) ^3", @"\sin \left( x\right) ^3")]
    [InlineData(@"\sin^3\ (x)", @"\sin \left( x\right) ^3", @"\sin \left( x\right) ^3")]
    [InlineData(@"\sin^3\; (x)", @"\sin \left( x\right) ^3", @"\sin \left( x\right) ^3")]
    [InlineData(@"\sin^3\ \; (x)", @"\sin \left( x\right) ^3", @"\sin \left( x\right) ^3")]
    [InlineData(@"\sin(x)^2", @"\sin \left( x\right) ^2", @"\sin \left( x\right) ^2")]
    [InlineData(@"\sin\ (x)^2", @"\sin \left( x\right) ^2", @"\sin \left( x\right) ^2")]
    [InlineData(@"\sin\; (x)^2", @"\sin \left( x\right) ^2", @"\sin \left( x\right) ^2")]
    [InlineData(@"\sin\ \; (x)^2", @"\sin \left( x\right) ^2", @"\sin \left( x\right) ^2")]
    [InlineData(@"\sin^a(x)", @"\sin \left( x\right) ^a", @"\sin \left( x\right) ^a")]
    [InlineData(@"\sin^a(x)^2", @"\sin \left( x\right) ^{a\times 2}", @"\sin \left( x\right) ^{2\times a}")]
    [InlineData(@"\sin^a\ (x)^2", @"\sin \left( x\right) ^{a\times 2}", @"\sin \left( x\right) ^{2\times a}")]
    [InlineData(@"\sin^a\; (x)^2", @"\sin \left( x\right) ^{a\times 2}", @"\sin \left( x\right) ^{2\times a}")]
    [InlineData(@"\sin^a\ \; (x)^2", @"\sin \left( x\right) ^{a\times 2}", @"\sin \left( x\right) ^{2\times a}")]
    [InlineData(@"\sin(x)^2(x)", @"\sin \left( x\right) ^2\times x", @"\sin \left( x\right) ^2\times x")]
    [InlineData(@"\sin\ (x)^2(x)", @"\sin \left( x\right) ^2\times x", @"\sin \left( x\right) ^2\times x")]
    [InlineData(@"\sin\; (x)^2(x)", @"\sin \left( x\right) ^2\times x", @"\sin \left( x\right) ^2\times x")]
    [InlineData(@"\sin\ \; (x)^2(x)", @"\sin \left( x\right) ^2\times x", @"\sin \left( x\right) ^2\times x")]
    [InlineData(@"\sin^a(x)^2(x)", @"\sin \left( x\right) ^{a\times 2}\times x", @"\sin \left( x\right) ^{2\times a}\times x")]
    [InlineData(@"\sin^a\ (x)^2(x)", @"\sin \left( x\right) ^{a\times 2}\times x", @"\sin \left( x\right) ^{2\times a}\times x")]
    [InlineData(@"\sin^a\; (x)^2(x)", @"\sin \left( x\right) ^{a\times 2}\times x", @"\sin \left( x\right) ^{2\times a}\times x")]
    [InlineData(@"\sin^a\ \; (x)^2(x)", @"\sin \left( x\right) ^{a\times 2}\times x", @"\sin \left( x\right) ^{2\times a}\times x")]
    public void Parentheses(string latex, string converted, string result) {
      Test(latex, converted, result);
      Test(latex.Replace("(", @"\left(").Replace(")", @"\right)"), converted, result);
    }
  }
}
