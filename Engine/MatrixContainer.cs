using MathNet.Numerics.LinearAlgebra.Single;
using System.Collections.Generic;

namespace Engine
{
    public class MatrixContainer
    {
        public int Dimensions { get; set; }
        public List<string> DocNameMap { get; set; }
        public List<string> Terms { get; set; }
        public DenseMatrix UMatrix { get; set; }
        public DenseMatrix VMatrix { get; set; }
        public float[,] DistanceMap { get; set; }
    }
}
