using System;
using System.Linq;

namespace ParserEngine
{	 
	public struct Complex : IComparable 
    {
        static readonly private double halfOfRoot2 = 0.5 * Math.Sqrt(2);
        static readonly public Complex One = new Complex(1, 0);
        static readonly public Complex Zero = new Complex(0, 0);
        static readonly public Complex DefaultZVector = new Complex(100, 0);
        static readonly public Complex I = new Complex(0, 1);
        static readonly public Complex MaxValue = new Complex(double.MaxValue, double.MaxValue);
        static readonly public Complex MinValue = new Complex(double.MinValue, double.MinValue);
        static readonly public Complex E = new Complex(Math.E, 0.0);
        static readonly public Complex PI = new Complex(Math.PI, 0.0);
        static readonly public Complex InvI = One / I;

        public double Re;         
        public double Im;
               
        public Complex(double real, double imaginary)
        {
            Re = real;
            Im = imaginary;
        }

        public Complex(Complex c)
        {
            Re = c.Re;
            Im = c.Im;
        }

        static public Complex CreateFromRealAndImaginary(double re, double im)
        {
            Complex c;
            c.Re = re;
            c.Im = im;
            return c;
        }
         
        static public Complex CreateFromModulusAndArgument(double mod, double arg)
        {
            Complex c;
            c.Re = mod * Math.Cos(arg);
            c.Im = mod * Math.Sin(arg);
            return c;
        }

        public static Complex Unit(Complex z)
        {
            return One;
        }

        [DerivMethodName(nameof(Unit))]
        public static Complex Ident(Complex z)
        {
            return z;
        }

        static public Complex Conjugate(Complex z)
        {
            z.Im = -z.Im;
            return z;
        }

        public static Complex Abs(Complex z)
        {
            z.Re = Math.Abs(z.Re);
            z.Im = Math.Abs(z.Im);
            return z;
        }

        public static Complex CAbs(Complex z)
        {
            z.Re = z.GetModulus();
            z.Im = 0;
            return z;
        }

        [DerivMethodName(nameof(TimesIDeriv))]
        static public Complex TimesI(Complex z)
        {
            return new Complex(-z.Im, z.Re);
        }

        [ExcludeMethod]
        public static Complex TimesIDeriv(Complex z)
        {
            return I;
        }

        [DerivMethodName(nameof(InverseDeriv))]
        public static Complex Inverse(Complex z)
        {
            return One / z;
        }

        [ExcludeMethod]
        public static Complex InverseDeriv(Complex z)
        {
            return -One / (z * z);
        }

        [DerivMethodName(nameof(SqrtDeriv))]
        static public Complex Sqrt(Complex c)
        {
            double x = c.Re;
            double y = c.Im;
            double modulus = Math.Sqrt(x * x + y * y);

            int sign = (y < 0) ? -1 : 1;

            c.Re = halfOfRoot2 * CMath.ZeroNaN(Math.Sqrt(modulus + x));
            c.Im = halfOfRoot2 * sign * CMath.ZeroNaN(Math.Sqrt(modulus - x));

            return c;
        }

        [ExcludeMethod]
        static public Complex SqrtDeriv(Complex c)
        {
            return 0.5 * One / Sqrt(c);
        }

        [DerivMethodName(nameof(SquareDeriv))]
        static public Complex Square(Complex c)
        {
            return c * c;
        }

        [ExcludeMethod]
        static public Complex SquareDeriv(Complex c)
        {
            return 2.0 * c;
        }

        static public Complex Pow(Complex c, double exponent)
        {
            Complex zP;
            if (exponent == 1.0)
                zP = c;
            else if (exponent == 2.0)
                zP = c * c;
            else if (exponent == 0.0)
                zP = Complex.One;
            else
            {
                double modulus = CMath.ZeroNaN(Math.Pow(c.Re * c.Re + c.Im * c.Im, exponent * 0.5));
                double argument = CMath.ZeroNaN(Math.Atan2(c.Im, c.Re) * exponent);

                c.Re = modulus * Math.Cos(argument);
                c.Im = modulus * Math.Sin(argument);

                zP = c;
            }
            return zP;
        }

        static public Complex Pow(Complex z, Complex exponent)
        {
            return Exp(exponent * Log(z));
        }

        [DerivMethodName(nameof(Exp))]
        static public Complex Exp(Complex c)
        {
            //e^a * (cos(b) + i * sin(b))
            double expRe = CMath.ZeroNaN(Math.Exp(c.Re));
            c.Re = expRe * Math.Cos(c.Im);
            c.Im = expRe * Math.Sin(c.Im);
            return c;
        }

        [DerivMethodName(nameof(Inverse))]
        public static Complex Log(Complex z)
        {
            double modulus = CMath.Log(z.GetModulus());
            double argument = CMath.ZeroNaN(Math.Atan2(z.Im, z.Re));
            z.Re = modulus;
            z.Im = argument;
            return z;
        }

        [DerivMethodName(nameof(Cos))]
        static public Complex Sin(Complex c)
        {
            Complex iz = TimesI(c);
            return 0.5 * InvI * (Exp(iz) - Exp(-iz));
        }

        [DerivMethodName(nameof(DerivCos))]
        static public Complex Cos(Complex c)
        {
            Complex iz = TimesI(c);
            return 0.5 * (Exp(iz) + Exp(-iz));
        }

        [ExcludeMethod]
        static public Complex DerivCos(Complex c)
        {
            return -Sin(c);
        }

        [DerivMethodName(nameof(DerivTan))]
        static public Complex Tan(Complex c)
        {
            return Sin(c) / Cos(c);
        }

        [ExcludeMethod]
        static public Complex DerivTan(Complex c)
        {
            return 1.0 + Square(Tan(c));
        }

        [DerivMethodName(nameof(CotDeriv))]
        static public Complex Cot(Complex c)
        {
            return Cos(c) / Sin(c);
        }

        [ExcludeMethod]
        static public Complex CotDeriv(Complex c)
        {
            return -One / Square(Sin(c));
        }

        [DerivMethodName(nameof(Cosh))]
        static public Complex Sinh(Complex c)
        {
            return 0.5 * (Exp(c) - Exp(-c));
        }

        [DerivMethodName(nameof(Sinh))]
        static public Complex Cosh(Complex c)
        {
            return 0.5 * (Exp(c) + Exp(-c));
        }

        [DerivMethodName(nameof(DerivTanh))]
        static public Complex Tanh(Complex c)
        {
            Complex z1 = Exp(c);
            Complex z2 = Exp(-c);
            return (z1 - z2) / (z1 + z2);
        }

        [ExcludeMethod]
        static public Complex DerivTanh(Complex c)
        {
            return 1.0 - Square(Tanh(c));
        }

        [DerivMethodName(nameof(DerivCoth))]
        static public Complex Coth(Complex c)
        {
            Complex z1 = Exp(c);
            Complex z2 = Exp(-c);
            return (z1 + z2) / (z1 - z2);
        }

        [ExcludeMethod]
        static public Complex DerivCoth(Complex c)
        {
            return 1.0 - Square(Coth(c));
        }

        //sinh(x + iy) = sinh(x) cos(y) + i cosh(x) sin(y)
        //static public Complex Sinh(Complex c)
        //{
        //    double re = Math.Sinh(c.Re) * Math.Cos(c.Im);
        //    double im = Math.Cosh(c.Re) * Math.Sin(c.Im);
        //    c.Re = re;
        //    c.Im = im;
        //    return c;
        //}

        public double GetModulus() 
        {
			return	Math.Sqrt( Re * Re + Im * Im );
		}
		 
		public double GetModulusSquared() 
        {			 
            return Re * Re + Im * Im;
		}
	 
		public double GetArgument() 
        {
			return Math.Atan2( Im, Re );
		}
	 	 
		public Complex GetConjugate() 
        {
			return CreateFromRealAndImaginary(Re, -Im);
		}
	  
		public void Normalize() 
        {
			double	modulus = this.GetModulus();
			//if( modulus == 0 ) 
   //         {
			//	throw new DivideByZeroException();
			//}
			Re	= Re / modulus;
            Im = Im / modulus;
		}
	  
		public static explicit operator Complex ( double d ) 
        {
			Complex c;
			c.Re	= d;
			c.Im	= 0;
			return c;
		}
         
		public static explicit operator double ( Complex c ) 
        {
			return c.Re;
		}
			 
		public static bool	operator==( Complex a, Complex b ) 
        {
			return	( a.Re == b.Re ) && ( a.Im == b.Im );
		}
		 
		public static bool	operator!=( Complex a, Complex b )
        {
			return	( a.Re != b.Re ) || ( a.Im != b.Im );
		}
	 
		public override int	GetHashCode() 
        {
			return	( Re.GetHashCode() ^ Im.GetHashCode() );
		}
 
		public override bool Equals( object o )
        {
			if( o is Complex ) 
            {
				Complex c = (Complex) o;
				return  ( this == c );
			}
			return	false;
		}
		 
		public int CompareTo( object o ) 
        {
			if( o == null ) 
            {
				return 1; 
			}
			else if( o is Complex ) 
            {
				return	this.GetModulus().CompareTo( ((Complex)o).GetModulus() );
			}
            else if (o is double) 
            {
				return	this.GetModulus().CompareTo( (double)o );
			}
            else if (o is float) 
            {
				return	this.GetModulus().CompareTo( (float)o );
			}
			throw new ArgumentException();
		}
	 	 
		public static Complex operator+( Complex a ) 
        {
			return a;
		}

        public static Complex operator +(Complex a, double f)
        {
            a.Re = a.Re + f;
            return a;
        }

        public static Complex operator +(double f, Complex a)
        {
            a.Re = a.Re + f;
            return a;
        }

        [MethodName("Sum")]
        public static Complex operator +(Complex a, Complex b)
        {
            a.Re = a.Re + b.Re;
            a.Im = a.Im + b.Im;
            return a;
        }

        [MethodName("Negative")]
		public static Complex operator-( Complex a ) 
        {
			a.Re	= -a.Re;
			a.Im	= -a.Im;
			return a;
		}
 	 
		public static Complex operator-( Complex a, double f ) 
        {
			a.Re	=  a.Re - f;
			return a;
		}
 
		public static Complex operator-( double f, Complex a ) 
        {
			a.Re	= f - a.Re;
			a.Im	= 0 - a.Im;
			return a;
		}

        [MethodName("Difference")]
        public static Complex operator-( Complex a, Complex b ) 
        {
			a.Re	= a.Re - b.Re;
			a.Im	= a.Im - b.Im;
			return a;
		}

        public static Complex operator*( Complex a, double f ) 
        {
			a.Re	= a.Re * f;
			a.Im	= a.Im * f;
			return a;
		}
			 
		public static Complex operator*( double f, Complex a ) 
        {
			a.Re	= a.Re * f;
			a.Im	= a.Im * f;			
			return a;
		}

        [MethodName("Product")]
        public static Complex operator*( Complex a, Complex b ) 
        {
			double	x = a.Re, y = a.Im;
			double	u = b.Re, v = b.Im;			
			a.Re	= x * u - y * v;
			a.Im	= x * v + y * u;
			
			return a;
		}
	 
		public static Complex operator/( Complex a, double f ) 
        {
			//if( f == 0 ) 
   //         {
			//	throw new DivideByZeroException();
			//}
			
			a.Re	= a.Re / f;
			a.Im	= a.Im / f;
			
			return a;
		}

        [MethodName("Quotient")]
        public static Complex operator/( Complex a, Complex b ) 
        {
			double	x = a.Re,	y = a.Im;
			double	u = b.Re,	v = b.Im;
			double	denom = u*u + v*v;

			a.Re	= ( x*u + y*v ) / denom;
			a.Im	= ( y*u - x*v ) / denom;
			
			return a;
		}
	  
		public override string ToString() 
        {
			return	$"({Re},{Im})";
		}

        public static bool TryParse(string s, out Complex z)
        {
            if (s != null)
            {
                s = s.Replace("(", "").Replace(")", "");
                string[] parts = s.Split(',').Select(p => p.Trim()).ToArray();
                if (parts.Length == 2 && double.TryParse(parts[0], out double re) && 
                                         double.TryParse(parts[1], out double im))
                {
                    z = new Complex(re, im);
                    return true;
                }
            }
            z = Zero;
            return false;
        }
	  
		static public bool IsEqual( Complex a, Complex b, double tolerance )
        {
		    return  Math.Abs( a.Re - b.Re ) < tolerance &&
				    Math.Abs( a.Im - b.Im ) < tolerance;
		}


    }

}
