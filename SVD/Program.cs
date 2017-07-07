using Engine;
using HtmlAgilityPack;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SVD
{
    public class Processor
    {
        public static SvdEntities SvdContext = new SvdEntities();

        static void Main(string[] args)
        {
            LSA.GetMatrixContainer();
            //LSA.ProcessAndStore();
            ClusterOptimizer.OptimizeRange(SvdContext, 5, 15, 2);
        }
    }
}
