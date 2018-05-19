using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathServer
{
    interface IMathService
    {
        double Add(double firstValue, double secondValue);

        double Sub(double firstValue, double secondValue);

        double Div(double firstValue, double secondValue);

        double Mult(double firstValue, double secondValue);
    }
}
